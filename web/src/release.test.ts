import { describe, expect, it } from "vitest";
import { openRelease } from "./release";
import type { PublicCorpus, PublicManifest } from "./types";

const corpus: PublicCorpus = {
  schemaVersion: "1",
  releaseId: "fixture-v1",
  generatedAtUtc: "2026-10-18T00:00:00.0000000+00:00",
  properties: [],
  integers: [],
};

describe("public release seam", () => {
  it("opens bytes only when identity and digest match the manifest", async () => {
    const bytes = new TextEncoder().encode(JSON.stringify(corpus));
    const manifest = await manifestFor(bytes);

    await expect(openRelease(bytes.buffer, manifest)).resolves.toEqual(corpus);
  });

  it("rejects bytes changed after the manifest was sealed", async () => {
    const bytes = new TextEncoder().encode(JSON.stringify(corpus));
    const manifest = await manifestFor(bytes);
    const changed = new TextEncoder().encode(JSON.stringify({ ...corpus, releaseId: "fixture-v2" }));

    await expect(openRelease(changed.buffer, manifest)).rejects.toThrow("digest");
  });
});

async function manifestFor(bytes: Uint8Array<ArrayBuffer>): Promise<PublicManifest> {
  const digest = await crypto.subtle.digest("SHA-256", bytes);
  const dataSha256 = [...new Uint8Array(digest)]
    .map((byte) => byte.toString(16).padStart(2, "0"))
    .join("");
  return {
    schemaVersion: corpus.schemaVersion,
    releaseId: corpus.releaseId,
    generatedAtUtc: corpus.generatedAtUtc,
    sourceDatabaseSchemaVersion: 1,
    dataSha256,
  };
}
