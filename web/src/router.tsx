import { useEffect, useState, type MouseEvent, type ReactNode } from "react";

export interface Route {
  path: string;
}

export function useRoute(): Route {
  const [path, setPath] = useState(() => fromBrowserPath(window.location.pathname));
  useEffect(() => {
    const update = () => setPath(fromBrowserPath(window.location.pathname));
    window.addEventListener("popstate", update);
    return () => window.removeEventListener("popstate", update);
  }, []);
  return { path };
}

export function navigate(path: string): void {
  const browserPath = toBrowserPath(path);
  if (window.location.pathname === browserPath) {
    return;
  }
  window.history.pushState({}, "", browserPath);
  window.dispatchEvent(new PopStateEvent("popstate"));
  const reducedMotion = window.matchMedia("(prefers-reduced-motion: reduce)").matches;
  window.scrollTo({ top: 0, behavior: reducedMotion ? "auto" : "smooth" });
}

export function fromBrowserPath(
  pathname: string,
  baseUrl = import.meta.env.BASE_URL,
): string {
  const basePath = normalizeBasePath(baseUrl);
  if (!basePath) return pathname || "/";
  if (pathname === basePath || pathname === `${basePath}/`) return "/";
  if (pathname.startsWith(`${basePath}/`)) return pathname.slice(basePath.length);
  return pathname || "/";
}

export function toBrowserPath(
  applicationPath: string,
  baseUrl = import.meta.env.BASE_URL,
): string {
  const basePath = normalizeBasePath(baseUrl);
  const path = applicationPath.startsWith("/") ? applicationPath : `/${applicationPath}`;
  if (!basePath) return path;
  return path === "/" ? `${basePath}/` : `${basePath}${path}`;
}

function normalizeBasePath(baseUrl: string): string {
  const withLeadingSlash = baseUrl.startsWith("/") ? baseUrl : `/${baseUrl}`;
  return withLeadingSlash.replace(/\/+$/, "");
}

export function Link({
  href,
  children,
  className,
  ariaLabel,
}: {
  href: string;
  children: ReactNode;
  className?: string;
  ariaLabel?: string;
}) {
  const follow = (event: MouseEvent<HTMLAnchorElement>) => {
    if (!event.defaultPrevented
      && event.button === 0
      && !event.metaKey
      && !event.ctrlKey
      && !event.shiftKey
      && !event.altKey) {
      event.preventDefault();
      navigate(href);
    }
  };
  return <a href={toBrowserPath(href)} onClick={follow} className={className} aria-label={ariaLabel}>{children}</a>;
}
