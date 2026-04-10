"use client";

import { cn } from "@/lib/utils";

// Full page loading spinner
export function PageLoader() {
  return (
    <div className="flex min-h-screen items-center justify-center bg-background">
      <div className="flex flex-col items-center gap-4">
        <LoadingSpinner size="lg" />
        <p className="text-muted-foreground">Loading...</p>
      </div>
    </div>
  );
}

// Loading spinner component
interface LoadingSpinnerProps {
  size?: "sm" | "md" | "lg";
  className?: string;
}

export function LoadingSpinner({
  size = "md",
  className,
}: LoadingSpinnerProps) {
  const sizeClasses = {
    sm: "h-4 w-4 border-2",
    md: "h-8 w-8 border-2",
    lg: "h-12 w-12 border-3",
  };

  return (
    <div
      className={cn(
        "animate-spin rounded-full border-primary border-t-transparent",
        sizeClasses[size],
        className
      )}
    />
  );
}

// Skeleton loading for cards
export function CardSkeleton() {
  return (
    <div className="rounded-lg border border-border bg-card p-6 animate-pulse">
      <div className="flex items-start gap-4">
        <div className="h-16 w-16 rounded-lg bg-muted" />
        <div className="flex-1 space-y-3">
          <div className="h-4 w-3/4 rounded bg-muted" />
          <div className="h-3 w-1/2 rounded bg-muted" />
        </div>
      </div>
    </div>
  );
}

// Skeleton loading for table rows
export function TableRowSkeleton() {
  return (
    <div className="flex items-center gap-4 border-b border-border px-4 py-3 animate-pulse">
      <div className="h-4 w-8 rounded bg-muted" />
      <div className="h-4 flex-1 rounded bg-muted" />
      <div className="h-4 w-24 rounded bg-muted" />
      <div className="h-4 w-16 rounded bg-muted" />
    </div>
  );
}

// Skeleton loading for stats cards
export function StatCardSkeleton() {
  return (
    <div className="rounded-lg border border-border bg-card p-6 animate-pulse">
      <div className="h-3 w-20 rounded bg-muted" />
      <div className="mt-3 h-8 w-24 rounded bg-muted" />
      <div className="mt-2 h-3 w-16 rounded bg-muted" />
    </div>
  );
}

// Skeleton for list items
export function ListItemSkeleton() {
  return (
    <div className="flex items-center gap-4 py-4 animate-pulse">
      <div className="h-12 w-12 rounded bg-muted" />
      <div className="flex-1 space-y-2">
        <div className="h-4 w-1/3 rounded bg-muted" />
        <div className="h-3 w-1/2 rounded bg-muted" />
      </div>
    </div>
  );
}

// Grid of skeleton cards
interface SkeletonGridProps {
  count?: number;
  columns?: 2 | 3 | 4;
}

export function SkeletonGrid({ count = 6, columns = 3 }: SkeletonGridProps) {
  const gridClass = {
    2: "grid-cols-1 md:grid-cols-2",
    3: "grid-cols-1 md:grid-cols-2 lg:grid-cols-3",
    4: "grid-cols-1 md:grid-cols-2 lg:grid-cols-4",
  };

  return (
    <div className={cn("grid gap-6", gridClass[columns])}>
      {Array.from({ length: count }).map((_, i) => (
        <CardSkeleton key={i} />
      ))}
    </div>
  );
}

// Empty state component
interface EmptyStateProps {
  icon?: React.ReactNode;
  title: string;
  description?: string;
  action?: React.ReactNode;
}

export function EmptyState({
  icon,
  title,
  description,
  action,
}: EmptyStateProps) {
  return (
    <div className="flex flex-col items-center justify-center py-16 text-center">
      {icon && (
        <div className="mb-4 flex h-16 w-16 items-center justify-center rounded-full bg-muted">
          {icon}
        </div>
      )}
      <h3 className="text-lg font-semibold text-foreground">{title}</h3>
      {description && (
        <p className="mt-2 max-w-sm text-muted-foreground">{description}</p>
      )}
      {action && <div className="mt-6">{action}</div>}
    </div>
  );
}
