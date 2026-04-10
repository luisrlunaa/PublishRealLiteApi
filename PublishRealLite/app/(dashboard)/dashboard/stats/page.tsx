"use client";

import { useEffect, useState } from "react";
import { apiClient } from "@/lib/api/client";
import type { StreamStatSummary } from "@/lib/api/types";
import { Button } from "@/components/ui/button";
import {
  StatCardSkeleton,
  EmptyState,
} from "@/components/loading-states";
import { AsyncError } from "@/components/error-boundary";
import {
  BarChart3,
  TrendingUp,
  Globe,
  Music,
  Calendar,
} from "lucide-react";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import {
  AreaChart,
  Area,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  ResponsiveContainer,
  BarChart,
  Bar,
  PieChart,
  Pie,
  Cell,
} from "recharts";

const COLORS = [
  "hsl(var(--chart-1))",
  "hsl(var(--chart-2))",
  "hsl(var(--chart-3))",
  "hsl(var(--chart-4))",
  "hsl(var(--chart-5))",
];

export default function StatsPage() {
  const [stats, setStats] = useState<StreamStatSummary | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<Error | null>(null);
  const [range, setRange] = useState("30");

  useEffect(() => {
    const fetchStats = async () => {
      setIsLoading(true);
      try {
        const data = await apiClient.getStatsSummary(parseInt(range));
        setStats(data);
      } catch (err) {
        setError(
          err instanceof Error ? err : new Error("Failed to load statistics")
        );
      } finally {
        setIsLoading(false);
      }
    };

    fetchStats();
  }, [range]);

  if (error) {
    return (
      <AsyncError error={error} onRetry={() => window.location.reload()} />
    );
  }

  const platformData = stats?.streamsByPlatform
    ? Object.entries(stats.streamsByPlatform).map(([name, value]) => ({
        name,
        value,
      }))
    : [];

  const countryData = stats?.streamsByCountry
    ? Object.entries(stats.streamsByCountry)
        .sort((a, b) => b[1] - a[1])
        .slice(0, 10)
        .map(([name, value]) => ({ name, value }))
    : [];

  const trendData = stats?.dailyTrends
    ? stats.dailyTrends.map((t) => ({
        date: new Date(t.date).toLocaleDateString("en-US", {
          month: "short",
          day: "numeric",
        }),
        streams: t.streams,
      }))
    : [];

  const hasData =
    stats &&
    (stats.totalStreams > 0 ||
      Object.keys(stats.streamsByPlatform || {}).length > 0);

  return (
    <div className="space-y-8">
      {/* Header */}
      <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <h1 className="text-3xl font-bold text-foreground">Statistics</h1>
          <p className="mt-1 text-muted-foreground">
            Track your streaming performance
          </p>
        </div>
        <div className="flex items-center gap-2">
          <Calendar className="h-4 w-4 text-muted-foreground" />
          <Select value={range} onValueChange={setRange}>
            <SelectTrigger className="w-[140px] bg-input">
              <SelectValue />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="7">Last 7 days</SelectItem>
              <SelectItem value="30">Last 30 days</SelectItem>
              <SelectItem value="90">Last 90 days</SelectItem>
              <SelectItem value="365">Last year</SelectItem>
            </SelectContent>
          </Select>
        </div>
      </div>

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
              color="primary"
            />
            <StatCard
              title="Platforms"
              value={Object.keys(stats?.streamsByPlatform || {}).length.toString()}
              icon={<Music className="h-5 w-5" />}
              color="chart-2"
            />
            <StatCard
              title="Countries"
              value={Object.keys(stats?.streamsByCountry || {}).length.toString()}
              icon={<Globe className="h-5 w-5" />}
              color="chart-3"
            />
            <StatCard
              title="Avg. Daily"
              value={formatNumber(
                stats?.dailyTrends
                  ? Math.round(
                      stats.dailyTrends.reduce((a, b) => a + b.streams, 0) /
                        stats.dailyTrends.length
                    )
                  : 0
              )}
              icon={<TrendingUp className="h-5 w-5" />}
              color="chart-4"
            />
          </>
        )}
      </div>

      {!isLoading && !hasData ? (
        <EmptyState
          icon={<BarChart3 className="h-8 w-8 text-muted-foreground" />}
          title="No streaming data yet"
          description="Once your music starts getting plays, you'll see your statistics here."
        />
      ) : (
        <>
          {/* Streams Over Time */}
          <div className="rounded-xl border border-border bg-card p-6">
            <h2 className="mb-4 text-lg font-semibold text-foreground">
              Streams Over Time
            </h2>
            {isLoading ? (
              <div className="h-[300px] animate-pulse bg-muted rounded" />
            ) : (
              <div className="h-[300px]">
                <ResponsiveContainer width="100%" height="100%">
                  <AreaChart data={trendData}>
                    <defs>
                      <linearGradient
                        id="streamGradient"
                        x1="0"
                        y1="0"
                        x2="0"
                        y2="1"
                      >
                        <stop
                          offset="5%"
                          stopColor="hsl(var(--primary))"
                          stopOpacity={0.3}
                        />
                        <stop
                          offset="95%"
                          stopColor="hsl(var(--primary))"
                          stopOpacity={0}
                        />
                      </linearGradient>
                    </defs>
                    <CartesianGrid
                      strokeDasharray="3 3"
                      stroke="hsl(var(--border))"
                    />
                    <XAxis
                      dataKey="date"
                      stroke="hsl(var(--muted-foreground))"
                      fontSize={12}
                    />
                    <YAxis
                      stroke="hsl(var(--muted-foreground))"
                      fontSize={12}
                      tickFormatter={(value) => formatNumber(value)}
                    />
                    <Tooltip
                      contentStyle={{
                        backgroundColor: "hsl(var(--card))",
                        border: "1px solid hsl(var(--border))",
                        borderRadius: "8px",
                      }}
                      labelStyle={{ color: "hsl(var(--foreground))" }}
                    />
                    <Area
                      type="monotone"
                      dataKey="streams"
                      stroke="hsl(var(--primary))"
                      fill="url(#streamGradient)"
                      strokeWidth={2}
                    />
                  </AreaChart>
                </ResponsiveContainer>
              </div>
            )}
          </div>

          <div className="grid gap-6 lg:grid-cols-2">
            {/* By Platform */}
            <div className="rounded-xl border border-border bg-card p-6">
              <h2 className="mb-4 text-lg font-semibold text-foreground">
                Streams by Platform
              </h2>
              {isLoading ? (
                <div className="h-[250px] animate-pulse bg-muted rounded" />
              ) : platformData.length > 0 ? (
                <div className="h-[250px]">
                  <ResponsiveContainer width="100%" height="100%">
                    <PieChart>
                      <Pie
                        data={platformData}
                        cx="50%"
                        cy="50%"
                        innerRadius={60}
                        outerRadius={90}
                        dataKey="value"
                        label={({ name, percent }) =>
                          `${name} ${(percent * 100).toFixed(0)}%`
                        }
                        labelLine={false}
                      >
                        {platformData.map((_, index) => (
                          <Cell
                            key={`cell-${index}`}
                            fill={COLORS[index % COLORS.length]}
                          />
                        ))}
                      </Pie>
                      <Tooltip
                        contentStyle={{
                          backgroundColor: "hsl(var(--card))",
                          border: "1px solid hsl(var(--border))",
                          borderRadius: "8px",
                        }}
                        formatter={(value: number) => [
                          formatNumber(value),
                          "Streams",
                        ]}
                      />
                    </PieChart>
                  </ResponsiveContainer>
                </div>
              ) : (
                <div className="flex h-[250px] items-center justify-center text-muted-foreground">
                  No platform data available
                </div>
              )}
            </div>

            {/* By Country */}
            <div className="rounded-xl border border-border bg-card p-6">
              <h2 className="mb-4 text-lg font-semibold text-foreground">
                Top Countries
              </h2>
              {isLoading ? (
                <div className="h-[250px] animate-pulse bg-muted rounded" />
              ) : countryData.length > 0 ? (
                <div className="h-[250px]">
                  <ResponsiveContainer width="100%" height="100%">
                    <BarChart data={countryData} layout="vertical">
                      <CartesianGrid
                        strokeDasharray="3 3"
                        stroke="hsl(var(--border))"
                      />
                      <XAxis
                        type="number"
                        stroke="hsl(var(--muted-foreground))"
                        fontSize={12}
                        tickFormatter={(value) => formatNumber(value)}
                      />
                      <YAxis
                        type="category"
                        dataKey="name"
                        stroke="hsl(var(--muted-foreground))"
                        fontSize={12}
                        width={60}
                      />
                      <Tooltip
                        contentStyle={{
                          backgroundColor: "hsl(var(--card))",
                          border: "1px solid hsl(var(--border))",
                          borderRadius: "8px",
                        }}
                        formatter={(value: number) => [
                          formatNumber(value),
                          "Streams",
                        ]}
                      />
                      <Bar
                        dataKey="value"
                        fill="hsl(var(--primary))"
                        radius={[0, 4, 4, 0]}
                      />
                    </BarChart>
                  </ResponsiveContainer>
                </div>
              ) : (
                <div className="flex h-[250px] items-center justify-center text-muted-foreground">
                  No country data available
                </div>
              )}
            </div>
          </div>
        </>
      )}
    </div>
  );
}

// Stat Card Component
interface StatCardProps {
  title: string;
  value: string;
  icon: React.ReactNode;
  color: string;
}

function StatCard({ title, value, icon, color }: StatCardProps) {
  return (
    <div className="rounded-xl border border-border bg-card p-6">
      <div className="flex items-center justify-between">
        <span className="text-sm font-medium text-muted-foreground">
          {title}
        </span>
        <div className={`text-${color}`}>{icon}</div>
      </div>
      <p className="mt-3 text-2xl font-bold text-foreground">{value}</p>
    </div>
  );
}

// Helper
function formatNumber(num: number): string {
  if (num >= 1000000) {
    return (num / 1000000).toFixed(1) + "M";
  }
  if (num >= 1000) {
    return (num / 1000).toFixed(1) + "K";
  }
  return num.toString();
}
