export type GraphMode = "none" | "shared-membership";

export interface PublicCorpus {
  schemaVersion: string;
  releaseId: string;
  generatedAtUtc: string;
  properties: PublicProperty[];
  integers: PublicInteger[];
}

export interface PublicManifest {
  schemaVersion: string;
  releaseId: string;
  generatedAtUtc: string;
  sourceDatabaseSchemaVersion: number;
  dataSha256: string;
}

export interface PublicProperty {
  slug: string;
  name: string;
  definition: string;
  aliases: string[];
  graphMode: GraphMode;
  coverage: PublicCoverage[];
  citations: PublicCitation[];
}

export interface PublicCoverage {
  minimum: string;
  maximum: string;
  basis: "deterministic" | "source-asserted-bounded";
  exclusions: string[];
}

export interface PublicInteger {
  value: string;
  claims: PublicClaim[];
  profileEntries: PublicProfileEntry[];
}

export interface PublicClaim {
  propertySlug: string;
  qualifier: Record<string, boolean | string | number>;
  witness: PublicWitness | null;
  citations: PublicCitation[];
}

export interface PublicWitness {
  kind: string;
  schemaVersion: number;
  displayText: string;
}

export interface PublicProfileEntry {
  slug: string;
  title: string;
  category: string;
  epistemicRegister: string;
  body: string;
  citations: PublicCitation[];
}

export interface PublicCitation {
  label: string;
  locator: string | null;
  role: string;
  url: string;
}

export interface CorpusIndex {
  corpus: PublicCorpus;
  properties: Map<string, PublicProperty>;
  integers: Map<string, PublicInteger>;
  members: Map<string, PublicInteger[]>;
}

export interface RelationReason {
  property: PublicProperty;
  centerClaim: PublicClaim;
  relatedClaim: PublicClaim;
}

export interface IntegerRelation {
  integer: PublicInteger;
  reasons: RelationReason[];
}

export type KnowledgeStatus = "positive" | "covered-negative" | "unassessed";
