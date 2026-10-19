import { readFile, readdir } from "node:fs/promises";
import { extname } from "node:path";

const packageJson = JSON.parse(await readFile(new URL("../package.json", import.meta.url), "utf8"));
const runtimeDependencies = Object.keys(packageJson.dependencies ?? {}).sort();
const expectedRuntimeDependencies = ["react", "react-dom"];
if (JSON.stringify(runtimeDependencies) !== JSON.stringify(expectedRuntimeDependencies)) {
  throw new Error(`unexpected public runtime dependencies: ${runtimeDependencies.join(", ")}`);
}

const files = await sourceFiles(new URL("../src/", import.meta.url));
const forbidden = [
  /microsoft\.data\.sqlite/i,
  /better-sqlite3/i,
  /\bsqlite3?\b/i,
  /polyvalent\.data/i,
  /databasepathresolver/i,
  /privatenotes/i,
  /sessionstorage/i,
  /localstorage/i,
  /\.env\b/i,
  /[a-z0-9._%+-]+@gmail\.com/i,
];
const fetchTargets = [];
let fetchCalls = 0;
let providerFetchCalls = 0;
for (const file of files) {
  const source = await readFile(file, "utf8");
  const boundarySource = source
    .replaceAll("import.meta.env.BASE_URL", "import.meta.BASE_URL")
    .replaceAll("import.meta.env.VITE_PUBLIC_SUGGESTION_FORM_ID", "import.meta.PUBLIC_SUGGESTION_FORM_ID");
  for (const pattern of forbidden) {
    if (pattern.test(boundarySource)) throw new Error(`${file.pathname} contains forbidden public-boundary token ${pattern}`);
  }
  const providerFetchPattern = /\(input,\s*init\)\s*=>\s*fetch\(input,\s*init\)/g;
  const providerFetchMatches = [...source.matchAll(providerFetchPattern)];
  providerFetchCalls += providerFetchMatches.length;
  const staticFetchSource = source.replace(providerFetchPattern, "");
  fetchCalls += staticFetchSource.match(/\bfetch\s*\(/g)?.length ?? 0;
  for (const match of staticFetchSource.matchAll(/fetch\(\s*["']([^"']+)["']/g)) {
    fetchTargets.push(match[1].replace(/^\//, ""));
  }
  for (const match of staticFetchSource.matchAll(/fetch\(\s*`\$\{import\.meta\.env\.BASE_URL\}([^`$]+)`/g)) {
    fetchTargets.push(match[1]);
  }
}

if (fetchTargets.length !== fetchCalls) {
  throw new Error(`public runtime contains ${fetchCalls - fetchTargets.length} non-static fetch target(s)`);
}

const allowedFetchTargets = new Set(["data/data.json", "data/manifest.json"]);
for (const target of fetchTargets) {
  if (!allowedFetchTargets.has(target)) throw new Error(`unexpected public runtime fetch target: ${target}`);
}
if (fetchTargets.length !== allowedFetchTargets.size) {
  throw new Error(`expected exactly ${allowedFetchTargets.size} local release fetches, found ${fetchTargets.length}`);
}

if (providerFetchCalls !== 1) {
  throw new Error(`expected exactly one bounded provider fetch adapter, found ${providerFetchCalls}`);
}

const suggestionSource = await readFile(new URL("../src/suggestions.ts", import.meta.url), "utf8");
for (const expected of [
  'const formIdPattern = /^[a-z0-9]{8,32}$/;',
  'const formspreeEndpointPrefix = "https://formspree.io/f/";',
  "request(suggestionEndpoint(formId)",
  'credentials: "omit"',
]) {
  if (!suggestionSource.includes(expected)) {
    throw new Error(`suggestion boundary is missing required constraint: ${expected}`);
  }
}

console.log(`public boundary ok: ${files.length} source files, ${runtimeDependencies.length} runtime dependencies, ${fetchTargets.length} local fetches, ${providerFetchCalls} bounded provider adapter`);

async function sourceFiles(directory) {
  const entries = await readdir(directory, { withFileTypes: true });
  const nested = await Promise.all(entries.map(async (entry) => {
    const child = new URL(`${entry.name}${entry.isDirectory() ? "/" : ""}`, directory);
    if (entry.isDirectory()) return sourceFiles(child);
    return [child];
  }));
  return nested.flat().filter((file) => [".ts", ".tsx"].includes(extname(file.pathname)));
}
