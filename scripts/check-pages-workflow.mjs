import { readFile } from "node:fs/promises";

const workflow = await readFile(
  new URL("../.github/workflows/deploy-pages.yml", import.meta.url),
  "utf8",
);

for (const pattern of [
  /secrets\./i,
  /\.env\b/i,
  /\.sqlite3?\b/i,
  /polyvalent\.publisher/i,
  /\bdotnet\b/i,
]) {
  if (pattern.test(workflow)) {
    throw new Error(`pages workflow contains forbidden private-boundary token ${pattern}`);
  }
}

const actionReferences = [...workflow.matchAll(/uses:\s+([^\s@]+)@([^\s#]+)/g)];
if (actionReferences.length !== 5) {
  throw new Error(`expected 5 action references, found ${actionReferences.length}`);
}
for (const [, action, reference] of actionReferences) {
  if (!/^[0-9a-f]{40}$/.test(reference)) {
    throw new Error(`${action} is not pinned to a full commit hash`);
  }
}

const repositoryVariables = [...workflow.matchAll(/\$\{\{\s*vars\.([A-Z0-9_]+)\s*\}\}/g)]
  .map((match) => match[1]);
if (JSON.stringify(repositoryVariables) !== JSON.stringify(["PUBLIC_SUGGESTION_FORM_ID"])) {
  throw new Error(`unexpected github repository variables: ${repositoryVariables.join(", ")}`);
}

const buildJob = job("build");
const deployJob = job("deploy");

requireText(workflow, "branches: [main]");
requireText(workflow, "workflow_dispatch:");
requireText(workflow, "cancel-in-progress: true");
requireText(buildJob, "contents: read");
requireText(buildJob, "pages: read");
requireText(buildJob, "persist-credentials: false");
requireText(buildJob, "./scripts/check-private-boundary.sh");
requireText(buildJob, "node ./scripts/check-pages-workflow.mjs");
requireText(buildJob, "npm run check:artifact");
requireText(buildJob, "npm run check:boundary");
requireText(buildJob, "steps.pages.outputs.base_path");
requireText(buildJob, "VITE_PUBLIC_SUGGESTION_FORM_ID: ${{ vars.PUBLIC_SUGGESTION_FORM_ID }}");
requireText(buildJob, "npm run check:dist-boundary");
requireText(buildJob, "cp dist/index.html dist/404.html");
requireText(buildJob, "path: web/dist");
requireText(workflow, "- scripts/check-pages-workflow.mjs");

const uploadPaths = [...buildJob.matchAll(/^\s+path:\s+(.+)$/gm)].map((match) => match[1]);
if (uploadPaths.length !== 1 || uploadPaths[0] !== "web/dist") {
  throw new Error(`pages workflow upload path widened: ${uploadPaths.join(", ")}`);
}

requireText(deployJob, "needs: build");
requireText(deployJob, "pages: write");
requireText(deployJob, "id-token: write");
requireText(deployJob, "name: github-pages");
if (/actions\/checkout|^\s+run:/m.test(deployJob)) {
  throw new Error("pages deploy job may not check out source or run shell commands");
}

console.log("pages workflow boundary ok: pinned actions, checked web/dist-only build, isolated deploy job");

function job(name) {
  const marker = `  ${name}:\n`;
  const start = workflow.indexOf(marker);
  if (start < 0) throw new Error(`missing pages workflow job ${name}`);
  const remainder = workflow.slice(start + marker.length);
  const nextJob = remainder.search(/^  [a-zA-Z0-9_-]+:\n/m);
  return nextJob < 0 ? remainder : remainder.slice(0, nextJob);
}

function requireText(section, expected) {
  if (!section.includes(expected)) {
    throw new Error(`pages workflow is missing required boundary: ${expected}`);
  }
}
