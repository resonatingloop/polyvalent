import { describe, expect, it } from "vitest";
import { fromBrowserPath, toBrowserPath } from "./router";

describe("deployment base routing", () => {
  it("keeps root-hosted development paths unchanged", () => {
    expect(fromBrowserPath("/integer/153", "/")).toBe("/integer/153");
    expect(toBrowserPath("/integer/153", "/")).toBe("/integer/153");
  });

  it("maps a github project path to application routes", () => {
    expect(fromBrowserPath("/polyvalent/", "/polyvalent/")).toBe("/");
    expect(fromBrowserPath("/polyvalent/integer/153", "/polyvalent/")).toBe("/integer/153");
    expect(fromBrowserPath("/polyvalent/property/prime", "/polyvalent/")).toBe("/property/prime");
  });

  it("puts application links beneath the github project path", () => {
    expect(toBrowserPath("/", "/polyvalent/")).toBe("/polyvalent/");
    expect(toBrowserPath("/integer/343", "/polyvalent/")).toBe("/polyvalent/integer/343");
    expect(toBrowserPath("property/prime", "/polyvalent/")).toBe("/polyvalent/property/prime");
  });

  it("does not disguise a path outside the configured mount", () => {
    expect(fromBrowserPath("/integer/153", "/polyvalent/")).toBe("/integer/153");
  });
});
