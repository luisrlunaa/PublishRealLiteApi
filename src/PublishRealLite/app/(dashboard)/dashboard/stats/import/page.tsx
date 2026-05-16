"use client";

import { useState } from "react";
import Link from "next/link";
import { useForm, useFieldArray } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { apiClient } from "@/lib/api/client";
import type { StreamStatImportDto } from "@/lib/api/types";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { LoadingSpinner } from "@/components/loading-states";
import { toast } from "sonner";
import {
  ArrowLeft,
  Upload,
  Plus,
  Trash2,
  FileSpreadsheet,
  BarChart3,
  AlertCircle,
} from "lucide-react";

const platforms = [
  "Spotify",
  "Apple Music",
  "YouTube Music",
  "Amazon Music",
  "Deezer",
  "Tidal",
  "SoundCloud",
  "Other",
];

const countries = [
  { code: "US", name: "United States" },
  { code: "GB", name: "United Kingdom" },
  { code: "DE", name: "Germany" },
  { code: "FR", name: "France" },
  { code: "CA", name: "Canada" },
  { code: "AU", name: "Australia" },
  { code: "BR", name: "Brazil" },
  { code: "MX", name: "Mexico" },
  { code: "JP", name: "Japan" },
  { code: "KR", name: "South Korea" },
  { code: "ES", name: "Spain" },
  { code: "IT", name: "Italy" },
  { code: "NL", name: "Netherlands" },
  { code: "SE", name: "Sweden" },
  { code: "NO", name: "Norway" },
  { code: "GLOBAL", name: "Global / All Countries" },
];

const statSchema = z.object({
  date: z.string().min(1, "Date is required"),
  platform: z.string().min(1, "Platform is required"),
  country: z.string().min(1, "Country is required"),
  streams: z.coerce.number().min(0, "Streams must be 0 or greater"),
  metricType: z.string().optional(),
  source: z.string().optional(),
});

const formSchema = z.object({
  stats: z.array(statSchema).min(1, "At least one stat entry is required"),
});

type FormData = z.infer<typeof formSchema>;

export default function StatsImportPage() {
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [importResult, setImportResult] = useState<{ imported: number } | null>(
    null
  );

  const {
    register,
    control,
    handleSubmit,
    reset,
    setValue,
    watch,
    formState: { errors },
  } = useForm<FormData>({
    resolver: zodResolver(formSchema),
    defaultValues: {
      stats: [
        {
          date: new Date().toISOString().split("T")[0],
          platform: "",
          country: "",
          streams: 0,
          metricType: "streams",
          source: "manual",
        },
      ],
    },
  });

  const { fields, append, remove } = useFieldArray({
    control,
    name: "stats",
  });

  const onSubmit = async (data: FormData) => {
    setIsSubmitting(true);
    setImportResult(null);

    try {
      const statsToImport: StreamStatImportDto[] = data.stats.map((stat) => ({
        date: stat.date,
        platform: stat.platform,
        country: stat.country,
        streams: stat.streams,
        metricType: stat.metricType || "streams",
        source: stat.source || "manual",
      }));

      const result = await apiClient.importStats(statsToImport);
      setImportResult(result);
      toast.success(`Successfully imported ${result.imported} stat entries`);

      // Reset form
      reset({
        stats: [
          {
            date: new Date().toISOString().split("T")[0],
            platform: "",
            country: "",
            streams: 0,
            metricType: "streams",
            source: "manual",
          },
        ],
      });
    } catch (err) {
      const message =
        err && typeof err === "object" && "message" in err
          ? (err as { message: string }).message
          : "Failed to import stats";
      toast.error(message);
    } finally {
      setIsSubmitting(false);
    }
  };

  const addRow = () => {
    append({
      date: new Date().toISOString().split("T")[0],
      platform: "",
      country: "",
      streams: 0,
      metricType: "streams",
      source: "manual",
    });
  };

  return (
    <div className="space-y-8">
      {/* Header */}
      <div className="flex items-center gap-4">
        <Link href="/dashboard/stats">
          <Button variant="ghost" size="icon">
            <ArrowLeft className="h-5 w-5" />
          </Button>
        </Link>
        <div>
          <h1 className="text-3xl font-bold text-foreground">Import Stats</h1>
          <p className="mt-1 text-muted-foreground">
            Manually import streaming statistics
          </p>
        </div>
      </div>

      {/* Success Message */}
      {importResult && (
        <div className="rounded-xl border border-emerald-500/50 bg-emerald-500/10 p-4">
          <div className="flex items-center gap-3">
            <BarChart3 className="h-5 w-5 text-emerald-500" />
            <p className="text-sm font-medium text-emerald-600 dark:text-emerald-400">
              Successfully imported {importResult.imported} stat entries. View
              your updated{" "}
              <Link href="/dashboard/stats" className="underline">
                statistics
              </Link>
              .
            </p>
          </div>
        </div>
      )}

      {/* Info Card */}
      <div className="rounded-xl border border-border bg-card p-6">
        <div className="flex items-start gap-4">
          <div className="flex h-10 w-10 shrink-0 items-center justify-center rounded-lg bg-primary/10">
            <FileSpreadsheet className="h-5 w-5 text-primary" />
          </div>
          <div>
            <h3 className="font-semibold text-foreground">
              Manual Stats Import
            </h3>
            <p className="mt-1 text-sm text-muted-foreground">
              Enter your streaming statistics manually. This is useful for
              importing data from distributor reports or tracking stats from
              platforms not automatically connected.
            </p>
          </div>
        </div>
      </div>

      {/* Import Form */}
      <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
        <div className="rounded-xl border border-border bg-card p-6">
          <div className="mb-4 flex items-center justify-between">
            <h2 className="text-lg font-semibold text-foreground">
              Stat Entries
            </h2>
            <Button type="button" variant="outline" size="sm" onClick={addRow}>
              <Plus className="mr-2 h-4 w-4" />
              Add Row
            </Button>
          </div>

          {errors.stats?.root && (
            <div className="mb-4 flex items-center gap-2 rounded-lg border border-destructive/50 bg-destructive/10 p-3 text-sm text-destructive">
              <AlertCircle className="h-4 w-4" />
              {errors.stats.root.message}
            </div>
          )}

          <div className="space-y-4">
            {/* Header Row */}
            <div className="hidden grid-cols-12 gap-3 text-sm font-medium text-muted-foreground lg:grid">
              <div className="col-span-2">Date *</div>
              <div className="col-span-2">Platform *</div>
              <div className="col-span-2">Country *</div>
              <div className="col-span-2">Streams *</div>
              <div className="col-span-2">Metric Type</div>
              <div className="col-span-1">Source</div>
              <div className="col-span-1"></div>
            </div>

            {/* Data Rows */}
            {fields.map((field, index) => (
              <div
                key={field.id}
                className="grid grid-cols-1 gap-3 rounded-lg border border-border bg-secondary/20 p-4 lg:grid-cols-12 lg:items-start lg:border-0 lg:bg-transparent lg:p-0"
              >
                {/* Date */}
                <div className="lg:col-span-2">
                  <Label className="mb-1.5 block lg:hidden">Date *</Label>
                  <Input
                    type="date"
                    {...register(`stats.${index}.date`)}
                    className="h-10 bg-input"
                  />
                  {errors.stats?.[index]?.date && (
                    <p className="mt-1 text-xs text-destructive">
                      {errors.stats[index].date?.message}
                    </p>
                  )}
                </div>

                {/* Platform */}
                <div className="lg:col-span-2">
                  <Label className="mb-1.5 block lg:hidden">Platform *</Label>
                  <Select
                    value={watch(`stats.${index}.platform`)}
                    onValueChange={(value) =>
                      setValue(`stats.${index}.platform`, value)
                    }
                  >
                    <SelectTrigger className="h-10 bg-input">
                      <SelectValue placeholder="Select platform" />
                    </SelectTrigger>
                    <SelectContent>
                      {platforms.map((platform) => (
                        <SelectItem key={platform} value={platform}>
                          {platform}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                  {errors.stats?.[index]?.platform && (
                    <p className="mt-1 text-xs text-destructive">
                      {errors.stats[index].platform?.message}
                    </p>
                  )}
                </div>

                {/* Country */}
                <div className="lg:col-span-2">
                  <Label className="mb-1.5 block lg:hidden">Country *</Label>
                  <Select
                    value={watch(`stats.${index}.country`)}
                    onValueChange={(value) =>
                      setValue(`stats.${index}.country`, value)
                    }
                  >
                    <SelectTrigger className="h-10 bg-input">
                      <SelectValue placeholder="Select country" />
                    </SelectTrigger>
                    <SelectContent>
                      {countries.map((country) => (
                        <SelectItem key={country.code} value={country.code}>
                          {country.name}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                  {errors.stats?.[index]?.country && (
                    <p className="mt-1 text-xs text-destructive">
                      {errors.stats[index].country?.message}
                    </p>
                  )}
                </div>

                {/* Streams */}
                <div className="lg:col-span-2">
                  <Label className="mb-1.5 block lg:hidden">Streams *</Label>
                  <Input
                    type="number"
                    min={0}
                    {...register(`stats.${index}.streams`)}
                    className="h-10 bg-input"
                    placeholder="0"
                  />
                  {errors.stats?.[index]?.streams && (
                    <p className="mt-1 text-xs text-destructive">
                      {errors.stats[index].streams?.message}
                    </p>
                  )}
                </div>

                {/* Metric Type */}
                <div className="lg:col-span-2">
                  <Label className="mb-1.5 block lg:hidden">Metric Type</Label>
                  <Select
                    value={watch(`stats.${index}.metricType`) || "streams"}
                    onValueChange={(value) =>
                      setValue(`stats.${index}.metricType`, value)
                    }
                  >
                    <SelectTrigger className="h-10 bg-input">
                      <SelectValue />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="streams">Streams</SelectItem>
                      <SelectItem value="downloads">Downloads</SelectItem>
                      <SelectItem value="views">Views</SelectItem>
                    </SelectContent>
                  </Select>
                </div>

                {/* Source */}
                <div className="lg:col-span-1">
                  <Label className="mb-1.5 block lg:hidden">Source</Label>
                  <Input
                    {...register(`stats.${index}.source`)}
                    className="h-10 bg-input"
                    placeholder="manual"
                  />
                </div>

                {/* Delete Button */}
                <div className="flex items-center justify-end lg:col-span-1 lg:justify-center">
                  <Button
                    type="button"
                    variant="ghost"
                    size="icon"
                    onClick={() => remove(index)}
                    disabled={fields.length === 1}
                    className="h-10 w-10 text-muted-foreground hover:text-destructive"
                  >
                    <Trash2 className="h-4 w-4" />
                  </Button>
                </div>
              </div>
            ))}
          </div>

          <div className="mt-4 flex justify-center">
            <Button
              type="button"
              variant="outline"
              size="sm"
              onClick={addRow}
              className="w-full sm:w-auto"
            >
              <Plus className="mr-2 h-4 w-4" />
              Add Another Row
            </Button>
          </div>
        </div>

        {/* Actions */}
        <div className="flex items-center justify-end gap-4">
          <Link href="/dashboard/stats">
            <Button type="button" variant="outline">
              Cancel
            </Button>
          </Link>
          <Button type="submit" disabled={isSubmitting}>
            {isSubmitting && <LoadingSpinner size="sm" className="mr-2" />}
            <Upload className="mr-2 h-4 w-4" />
            {isSubmitting ? "Importing..." : "Import Stats"}
          </Button>
        </div>
      </form>
    </div>
  );
}
