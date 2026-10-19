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
if (data.properties.length !== 9) throw new Error(`expected 9 public properties, found ${data.properties.length}`);

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
const oneThirtySeven = values.get("137");
if (!oneFiftyThree || !threeFortyThree || !oneThirtySeven) throw new Error("reference integers are missing");
const shared = oneFiftyThree.claims
  .map((claim) => claim.propertySlug)
  .filter((slug) => threeFortyThree.claims.some((claim) => claim.propertySlug === slug));
if (!shared.includes("decimal-friedman-number")) throw new Error("reference friedman relation is missing");

const properties = new Map(data.properties.map((property) => [property.slug, property]));
for (const slug of ["prime", "perfect-square", "perfect-cube", "fibonacci-number", "twin-prime"]) {
  if (properties.get(slug)?.graphMode !== "shared-membership") {
    throw new Error(`${slug} is not graphable by shared membership`);
  }
}

const claim = (integer, slug) => integer.claims.find((candidate) => candidate.propertySlug === slug);
const twinClaim = claim(oneThirtySeven, "twin-prime");
if (!claim(oneThirtySeven, "prime") || !twinClaim) throw new Error("137 is missing prime affiliations");
if (twinClaim.qualifier.higherTwin !== "139" || twinClaim.witness?.displayText !== "twin with 139") {
  throw new Error("137 is missing its exact twin-prime witness");
}
if (claim(threeFortyThree, "perfect-cube")?.witness?.displayText !== "7^3 = 343") {
  throw new Error("343 is missing its perfect-cube witness");
}
if (!properties.get("twin-prime")?.citations.some((citation) => citation.url === "https://oeis.org/A001097")) {
  throw new Error("twin-prime provenance is missing");
}

console.log(`public artifact ok: ${data.properties.length} properties, ${data.integers.length} integers, ${digest}`);
