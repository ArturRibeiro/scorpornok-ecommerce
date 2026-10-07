import { Button } from "@/components/ui/button";
import { cn } from "@/lib/utils";
import type { PageInfo } from "@/types/product";
import { ChevronLeft, ChevronRight } from "lucide-react";
import { Link } from "react-router";

// Páginas visíveis: a primeira, a última e duas de cada lado da atual;
// os intervalos pulados viram reticências.
function visiblePages(current: number, total: number): (number | "...")[] {
  const pages: (number | "...")[] = [];
  for (let page = 1; page <= total; page++) {
    if (page === 1 || page === total || Math.abs(page - current) <= 2) {
      pages.push(page);
    } else if (pages[pages.length - 1] !== "...") {
      pages.push("...");
    }
  }
  return pages;
}

const pageLink = (page: number) => ({ search: page > 1 ? `?page=${page}` : "" });

export default function ProductPagination({ pageInfo }: { pageInfo: PageInfo }) {
  const { currentPage, totalPages, pageSize, totalItems } = pageInfo;
  if (totalPages <= 1) return null;

  const first = (currentPage - 1) * pageSize + 1;
  const last = Math.min(currentPage * pageSize, totalItems);

  return (
    <nav
      aria-label="Product pages"
      className="max-w-7xl mx-auto mt-12 flex flex-col items-center gap-4"
    >
      <p className="text-sm text-muted-foreground">
        Showing {first}–{last} of {totalItems} products
      </p>

      <div className="flex flex-wrap items-center justify-center gap-1">
        <PageButton page={currentPage - 1} disabled={!pageInfo.hasPreviousPage}>
          <ChevronLeft />
          <span className="sr-only sm:not-sr-only">Previous</span>
        </PageButton>

        {visiblePages(currentPage, totalPages).map((page, index) =>
          page === "..." ? (
            <span
              key={`gap-${index}`}
              className="w-9 text-center text-muted-foreground"
            >
              …
            </span>
          ) : (
            <PageButton
              key={page}
              page={page}
              active={page === currentPage}
            >
              {page}
            </PageButton>
          )
        )}

        <PageButton page={currentPage + 1} disabled={!pageInfo.hasNextPage}>
          <span className="sr-only sm:not-sr-only">Next</span>
          <ChevronRight />
        </PageButton>
      </div>
    </nav>
  );
}

function PageButton({
  page,
  active = false,
  disabled = false,
  children,
}: {
  page: number;
  active?: boolean;
  disabled?: boolean;
  children: React.ReactNode;
}) {
  const className = cn("min-w-9", active && "pointer-events-none");
  const variant = active ? "default" : "ghost";

  if (disabled) {
    return (
      <Button variant={variant} className={className} disabled>
        {children}
      </Button>
    );
  }

  return (
    <Button variant={variant} className={className} asChild>
      <Link to={pageLink(page)} aria-current={active ? "page" : undefined}>
        {children}
      </Link>
    </Button>
  );
}
