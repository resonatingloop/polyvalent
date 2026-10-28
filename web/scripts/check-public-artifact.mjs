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
if (data.properties.length !== 17) throw new Error(`expected 17 public properties, found ${data.properties.length}`);
if (data.integers.length !== 541) throw new Error(`expected 541 public integers, found ${data.integers.length}`);
const claimCount = data.integers.reduce((sum, integer) => sum + integer.claims.length, 0);
const profileCount = data.integers.reduce((sum, integer) => sum + integer.profileEntries.length, 0);
if (claimCount !== 761) throw new Error(`expected 761 public claims, found ${claimCount}`);
if (profileCount !== 20) throw new Error(`expected 20 public profiles, found ${profileCount}`);

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
const oneThousandTwentyFour = values.get("1024");
const seventeenTwentyNine = values.get("1729");
const twoTwenty = values.get("220");
const twoEightyFour = values.get("284");
const eighteenOhSix = values.get("1806");
const eighteenOhSeven = values.get("1807");
if (!oneFiftyThree || !threeFortyThree || !oneThirtySeven || !oneThousandTwentyFour
    || !seventeenTwentyNine || !twoTwenty || !twoEightyFour || !eighteenOhSix || !eighteenOhSeven) {
  throw new Error("reference integers are missing");
}
const shared = oneFiftyThree.claims
  .map((claim) => claim.propertySlug)
  .filter((slug) => threeFortyThree.claims.some((claim) => claim.propertySlug === slug));
if (!shared.includes("decimal-friedman-number")) throw new Error("reference friedman relation is missing");

const properties = new Map(data.properties.map((property) => [property.slug, property]));
for (const slug of [
  "prime",
  "perfect-square",
  "perfect-cube",
  "fibonacci-number",
  "power-of-two",
  "twin-prime",
  "carmichael-number",
  "base-2-fermat-pseudoprime",
  "amicable-number",
  "sylvester-sequence-member",
  "primary-pseudoperfect-number",
  "heegner-number",
  "two-way-taxicab-number",
]) {
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
if (claim(oneThousandTwentyFour, "power-of-two")?.witness?.displayText !== "2^10 = 1024") {
  throw new Error("1024 is missing its power-of-two witness");
}
if (!properties.get("power-of-two")?.citations.some((citation) => citation.url === "https://oeis.org/A000079")) {
  throw new Error("power-of-two provenance is missing");
}
if (!properties.get("twin-prime")?.citations.some((citation) => citation.url === "https://oeis.org/A001097")) {
  throw new Error("twin-prime provenance is missing");
}
for (const [slug, accession] of [
  ["carmichael-number", "A002997"],
  ["base-2-fermat-pseudoprime", "A001567"],
  ["amicable-number", "A063990"],
  ["sylvester-sequence-member", "A000058"],
  ["primary-pseudoperfect-number", "A054377"],
  ["heegner-number", "A003173"],
  ["two-way-taxicab-number", "A001235"],
]) {
  if (!properties.get(slug)?.citations.some((citation) => citation.url === `https://oeis.org/${accession}`)) {
    throw new Error(`${slug} provenance is missing`);
  }
}
if (properties.get("heegner-number")?.coverage[0]?.basis !== "source-asserted-bounded") {
  throw new Error("heegner coverage is not visibly source-asserted");
}
if (claim(twoTwenty, "amicable-number")?.qualifier.partner !== "284"
    || claim(twoTwenty, "amicable-number")?.witness?.displayText !== "s(220) = 284; s(284) = 220") {
  throw new Error("220 is missing its exact amicable partner evidence");
}
if (claim(twoEightyFour, "amicable-number")?.qualifier.partner !== "220") {
  throw new Error("284 is missing its reciprocal amicable partner");
}
if (claim(seventeenTwentyNine, "two-way-taxicab-number")?.witness?.displayText
    !== "1^3 + 12^3 = 9^3 + 10^3 = 1729") {
  throw new Error("1729 is missing its complete two-way taxicab witness");
}
const seventeenTwentyNineNewClaims = [
  "carmichael-number",
  "base-2-fermat-pseudoprime",
  "two-way-taxicab-number",
];
if (!seventeenTwentyNineNewClaims.every((slug) => claim(seventeenTwentyNine, slug))) {
  throw new Error("1729 is missing network-ignition affiliations");
}
const graphableSlugs = new Set(
  data.properties.filter((property) => property.graphMode === "shared-membership").map((property) => property.slug),
);
const eighteenOhSixClaims = new Set(eighteenOhSix.claims.map((item) => item.propertySlug));
if (eighteenOhSeven.claims.some((item) => graphableSlugs.has(item.propertySlug) && eighteenOhSixClaims.has(item.propertySlug))) {
  throw new Error("1806 and 1807 acquired an invented relation");
}
if (data.integers.filter((integer) => claim(integer, "two-way-taxicab-number")).length !== 1) {
  throw new Error("two-way taxicab is not a singleton inside the bounded release");
}

const expectedProfiles = [
  ["137", "fine-structure-near-integer", 1],
  ["153", "vesica-and-unbroken-net", 3],
  ["93", "thelemic-greeting", 1],
  ["146", "lucina-minor-planet", 2],
  ["404", "http-not-found", 1],
  ["808", "tr-808-afterlife", 1],
  ["86", "eighty-sixed", 1],
  ["137", "paulis-last-room", 2],
  ["153", "sonnet-153", 1],
  ["36", "thirty-six-righteous", 1],
  ["88", "constellations-standardized", 1],
  ["108", "panchen-lama-prayer-beads", 1],
  ["156", "babalons-number", 1],
  ["163", "gardners-almost-integer", 1],
  ["231", "sefer-yetzirah-gates", 1],
  ["318", "abrahams-army-cipher", 2],
  ["343", "guilty-spark", 3],
  ["418", "abrahadabra-great-work", 1],
  ["616", "beasts-variant-number", 2],
  ["1729", "ramanujans-taxi", 1],
];
for (const [value, slug, citationCount] of expectedProfiles) {
  const integer = values.get(value);
  const profile = integer?.profileEntries.find((candidate) => candidate.slug === slug);
  if (!profile || profile.citations.length !== citationCount) {
    throw new Error(`${value} is missing its sourced ${slug} profile`);
  }
}
for (const value of ["86", "93", "108", "146", "156", "318", "418"]) {
  if (values.get(value).claims.length !== 0) throw new Error(`profile lore created an accidental claim for ${value}`);
}
if (!values.get("153").profileEntries.some((profile) => profile.body.includes("without claiming that one encodes the other"))) {
  throw new Error("153 is missing its non-encoding boundary");
}
if (raw.toLowerCase().includes("scholar's 666") || raw.toLowerCase().includes("scholar’s 666")) {
  throw new Error("private glosso provenance reached the public artifact");
}

console.log(`public artifact ok: ${data.properties.length} properties, ${data.integers.length} integers, ${digest}`);
