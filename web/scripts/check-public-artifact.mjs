import { createHash } from "node:crypto";
import { readFile } from "node:fs/promises";

const dataPath = new URL("../public/data/data.json", import.meta.url);
const manifestPath = new URL("../public/data/manifest.json", import.meta.url);
const [dataBytes, manifestBytes] = await Promise.all([readFile(dataPath), readFile(manifestPath)]);
const data = JSON.parse(dataBytes.toString("utf8"));
const manifest = JSON.parse(manifestBytes.toString("utf8"));
const digest = createHash("sha256").update(dataBytes).digest("hex");

if (manifest.dataSha256 !== digest) throw new Error("manifest digest does not match data.json");
if (data.releaseId !== manifest.releaseId) throw new Error("release ids disagree");
if (data.schemaVersion !== manifest.schemaVersion) throw new Error("schema versions disagree");
if (!Array.isArray(data.properties) || !Array.isArray(data.integers)) throw new Error("public arrays are missing");

const raw = dataBytes.toString("utf8");
for (const forbidden of [
  "privateNotes",
  "localPrivatePath",
  "sourceCaptureId",
  "populationRunId",
  "propertyVersionId",
  "claimRevisionId",
]) {
  if (raw.includes(`\"${forbidden}\"`)) throw new Error(`private field ${forbidden} reached data.json`);
}

const values = new Map(data.integers.map((integer) => [integer.value, integer]));
const oneFiftyThree = values.get("153");
const threeFortyThree = values.get("343");
if (!oneFiftyThree || !threeFortyThree) throw new Error("reference integers are missing");
const shared = oneFiftyThree.claims
  .map((claim) => claim.propertySlug)
  .filter((slug) => threeFortyThree.claims.some((claim) => claim.propertySlug === slug));
if (!shared.includes("decimal-friedman-number")) throw new Error("reference friedman relation is missing");

console.log(`public artifact ok: ${data.properties.length} properties, ${data.integers.length} integers, ${digest}`);
