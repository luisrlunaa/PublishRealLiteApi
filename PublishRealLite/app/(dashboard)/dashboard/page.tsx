"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import { useAuth } from "@/lib/auth/auth-context";
import { apiClient } from "@/lib/api/client";
import type { ReleaseDto, StreamStatSummary } from "@/lib/api/types";
import { Button } from "@/components/ui/button";
import {
  StatCardSkeleton,
  CardSkeleton,
  EmptyState,
} from "@/components/loading-states";
import { AsyncError } from "@/components/error-boundary";
import {
  Plus,
  Disc3,
  BarChart3,
  TrendingUp,
  Music,
  ArrowRight,
  User,
} from "lucide-react";

export default function DashboardPage() {
  const { profile } = useAuth();
  const [releases, setReleases] = useState<ReleaseDto[]>([]);
  const [stats, setStats] = useState<StreamStatSummary | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<Error | null>(null);

  useEffect(() => {
    const fetchData = async () => {
      try {
        const [releasesData, statsData] = await Promise.all([
          apiClient.getReleases().catch(() => []),
          apiClient.getStatsSummary(30).catch(() => null),
        ]);
        setReleases(releasesData);
        setStats(statsData);
      } catch (err) {
        setError(err instanceof Error ? err : new Error("Failed to load data"));
      } finally {
        setIsLoading(false);
      }
    };

    fetchData();
  }, []);

  if (error) {
    return (
      <AsyncError error={error} onRetry={() => window.location.reload()} />
    );
  }

  return (
    <div className="space-y-8">
      {/* Header */}
      <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <h1 className="text-3xl font-bold text-foreground">
            Welcome back{profile?.artistName ? `, ${profile.artistName}` : ""}
          </h1>
          <p className="mt-1 text-muted-foreground">
            {"Here's what's happening with your music"}
          </p>
        </div>
        <Link href="/dashboard/releases/new">
          <Button className="font-semibold">
            <Plus className="mr-2 h-4 w-4" />
            New Release
          </Button>
        </Link>
      </div>

      {/* Profile prompt if no profile */}
      {!profile && (
        <div className="rounded-xl border border-primary/50 bg-primary/10 p-6">
          <div className="flex items-start gap-4">
            <div className="flex h-12 w-12 items-center justify-center rounded-full bg-primary">
              <User className="h-6 w-6 text-primary-foreground" />
            </div>
            <div className="flex-1">
              <h3 className="font-semibold text-foreground">
                Complete your artist profile
              </h3>
              <p className="mt-1 text-sm text-muted-foreground">
                Set up your artist profile to start uploading music and tracking
                your stats.
              </p>
              <Link href="/dashboard/profile">
                <Button variant="outline" className="mt-4">
                  Create Profile
                  <ArrowRight className="ml-2 h-4 w-4" />
                </Button>
              </Link>
            </div>
          </div>
        </div>
      )}

      {/* Stats Grid */}
      <div className="grid gap-6 sm:grid-cols-2 lg:grid-cols-4">
        {isLoading ? (
          <>
            <StatCardSkeleton />
            <StatCardSkeleton />
            <StatCardSkeleton />
            <StatCardSkeleton />
          </>
        ) : (
          <>
            <StatCard
              title="Total Streams"
              value={formatNumber(stats?.totalStreams || 0)}
              icon={<BarChart3 className="h-5 w-5" />}
              trend="+12%"
            />
            <StatCard
              title="Releases"
              value={releases.length.toString()}
              icon={<Disc3 className="h-5 w-5" />}
            />
            <StatCard
              title="Top Platform"
              value={getTopPlatform(stats?.streamsByPlatform)}
              icon={<Music className="h-5 w-5" />}
            />
            <StatCard
              title="Growth"
              value={calculateGrowth(stats?.dailyTrends)}
              icon={<TrendingUp className="h-5 w-5" />}
              trend="vs last month"
            />
          </>
        )}
      </div>

      {/* Recent Releases */}
      <div>
        <div className="mb-4 flex items-center justify-between">
          <h2 className="text-xl font-semibold text-foreground">
            Recent Releases
          </h2>
          <Link
            href="/dashboard/releases"
            className="text-sm font-medium text-primary hover:underline"
          >
            View all
          </Link>
        </div>

        {isLoading ? (
          <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
            <CardSkeleton />
            <CardSkeleton />
            <CardSkeleton />
          </div>
        ) : releases.length === 0 ? (
          <EmptyState
            icon={<Disc3 className="h-8 w-8 text-muted-foreground" />}
            title="No releases yet"
            description="Upload your first release and share your music with the world."
            action={
              <Link href="/dashboard/releases/new">
                <Button>
                  <Plus className="mr-2 h-4 w-4" />
                  Create Release
                </Button>
              </Link>
            }
          />
        ) : (
          <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
            {releases.slice(0, 6).map((release) => (
              <ReleaseCard key={release.id} release={release} />
            ))}
          </div>
        )}
      </div>
    </div>
  );
}

// Components

interface StatCardProps {
  title: string;
  value: string;
  icon: React.ReactNode;
  trend?: string;
}

function StatCard({ title, value, icon, trend }: StatCardProps) {
  return (
    <div className="rounded-xl border border-border bg-card p-6">
      <div className="flex items-center justify-between">
        <span className="text-sm font-medium text-muted-foreground">
          {title}
        </span>
        <div className="text-primary">{icon}</div>
      </div>
      <p className="mt-3 text-2xl font-bold text-foreground">{value}</p>
      {trend && (
        <p className="mt-1 text-sm text-muted-foreground">{trend}</p>
      )}
    </div>
  );
}

function ReleaseCard({ release }: { release: ReleaseDto }) {
  return (
    <Link
      href={`/dashboard/releases/${release.id}`}
      className="group rounded-xl border border-border bg-card p-4 transition-colors hover:border-primary/50"
    >
      <div className="flex items-start gap-4">
        <div className="flex h-16 w-16 items-center justify-center rounded-lg bg-secondary">
          <Disc3 className="h-8 w-8 text-muted-foreground" />
        </div>
        <div className="flex-1 overflow-hidden">
          <h3 className="truncate font-semibold text-foreground group-hover:text-primary">
            {release.title}
          </h3>
          <p className="mt-1 text-sm text-muted-foreground">
            {release.genre || "No genre"}
          </p>
          {release.releaseDate && (
            <p className="mt-1 text-xs text-muted-foreground">
              {new Date(release.releaseDate).toLocaleDateString()}
            </p>
          )}
        </div>
      </div>
    </Link>
  );
}

// Helpers

function formatNumber(num: number): string {
  if (num >= 1000000) {
    return (num / 1000000).toFixed(1) + "M";
  }
  if (num >= 1000) {
    return (num / 1000).toFixed(1) + "K";
  }
  return num.toString();
}

function getTopPlatform(
  platforms: Record<string, number> | undefined
): string {
  if (!platforms || Object.keys(platforms).length === 0) {
    return "N/A";
  }
  const sorted = Object.entries(platforms).sort((a, b) => b[1] - a[1]);
  return sorted[0][0];
}

function calculateGrowth(
  trends: { date: string; streams: number }[] | undefined
): string {
  if (!trends || trends.length < 2) {
    return "0%";
  }
  const recent = trends.slice(-7).reduce((sum, t) => sum + t.streams, 0);
  const previous = trends.slice(-14, -7).reduce((sum, t) => sum + t.streams, 0);
  if (previous === 0) return "+100%";
  const growth = ((recent - previous) / previous) * 100;
  return (growth >= 0 ? "+" : "") + growth.toFixed(0) + "%";
}
