"use client";

import { useEffect, useState } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { apiClient } from "@/lib/api/client";
import type { ArtistVideo } from "@/lib/api/types";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import {
  SkeletonGrid,
  EmptyState,
  LoadingSpinner,
} from "@/components/loading-states";
import { AsyncError } from "@/components/error-boundary";
import { toast } from "sonner";
import {
  Plus,
  Video,
  MoreHorizontal,
  Pencil,
  Trash2,
  ExternalLink,
  X,
} from "lucide-react";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from "@/components/ui/alert-dialog";

const videoSchema = z.object({
  title: z.string().min(1, "Title is required"),
  videoUrl: z.string().url("Please enter a valid URL"),
  thumbnailUrl: z.string().url("Please enter a valid URL").optional().or(z.literal("")),
});

type VideoFormData = z.infer<typeof videoSchema>;

export default function VideosPage() {
  const [videos, setVideos] = useState<ArtistVideo[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<Error | null>(null);
  const [showDialog, setShowDialog] = useState(false);
  const [editingVideo, setEditingVideo] = useState<ArtistVideo | null>(null);
  const [deleteId, setDeleteId] = useState<number | null>(null);
  const [isSaving, setIsSaving] = useState(false);

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<VideoFormData>({
    resolver: zodResolver(videoSchema),
  });

  useEffect(() => {
    fetchVideos();
  }, []);

  const fetchVideos = async () => {
    try {
      const data = await apiClient.getMyVideos();
      setVideos(data);
    } catch (err) {
      setError(
        err instanceof Error ? err : new Error("Failed to load videos")
      );
    } finally {
      setIsLoading(false);
    }
  };

  const openCreateDialog = () => {
    setEditingVideo(null);
    reset({ title: "", videoUrl: "", thumbnailUrl: "" });
    setShowDialog(true);
  };

  const openEditDialog = (video: ArtistVideo) => {
    setEditingVideo(video);
    reset({
      title: video.title,
      videoUrl: video.videoUrl,
      thumbnailUrl: video.thumbnailUrl || "",
    });
    setShowDialog(true);
  };

  const onSubmit = async (data: VideoFormData) => {
    setIsSaving(true);

    try {
      if (editingVideo) {
        await apiClient.updateVideo(editingVideo.id, {
          title: data.title,
          videoUrl: data.videoUrl,
          thumbnailUrl: data.thumbnailUrl,
        });
        setVideos((prev) =>
          prev.map((v) =>
            v.id === editingVideo.id ? { ...v, ...data } : v
          )
        );
        toast.success("Video updated successfully");
      } else {
        const newVideo = await apiClient.createVideo({
          title: data.title,
          videoUrl: data.videoUrl,
          thumbnailUrl: data.thumbnailUrl,
        });
        setVideos((prev) => [newVideo, ...prev]);
        toast.success("Video added successfully");
      }
      setShowDialog(false);
    } catch (err) {
      const message =
        err && typeof err === "object" && "message" in err
          ? (err as { message: string }).message
          : "Failed to save video";
      toast.error(message);
    } finally {
      setIsSaving(false);
    }
  };

  const handleDelete = async () => {
    if (!deleteId) return;

    try {
      await apiClient.deleteVideo(deleteId);
      setVideos((prev) => prev.filter((v) => v.id !== deleteId));
      toast.success("Video deleted successfully");
    } catch {
      toast.error("Failed to delete video");
    } finally {
      setDeleteId(null);
    }
  };

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
          <h1 className="text-3xl font-bold text-foreground">Videos</h1>
          <p className="mt-1 text-muted-foreground">
            Manage your promotional videos and content
          </p>
        </div>
        <Button className="font-semibold" onClick={openCreateDialog}>
          <Plus className="mr-2 h-4 w-4" />
          Add Video
        </Button>
      </div>

      {/* Videos Grid */}
      {isLoading ? (
        <SkeletonGrid count={6} columns={3} />
      ) : videos.length === 0 ? (
        <EmptyState
          icon={<Video className="h-8 w-8 text-muted-foreground" />}
          title="No videos yet"
          description="Add promotional videos, music videos, and behind-the-scenes content."
          action={
            <Button onClick={openCreateDialog}>
              <Plus className="mr-2 h-4 w-4" />
              Add Video
            </Button>
          }
        />
      ) : (
        <div className="grid gap-6 sm:grid-cols-2 lg:grid-cols-3">
          {videos.map((video) => (
            <VideoCard
              key={video.id}
              video={video}
              onEdit={() => openEditDialog(video)}
              onDelete={() => setDeleteId(video.id)}
            />
          ))}
        </div>
      )}

      {/* Add/Edit Dialog */}
      <Dialog open={showDialog} onOpenChange={setShowDialog}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>
              {editingVideo ? "Edit Video" : "Add Video"}
            </DialogTitle>
          </DialogHeader>
          <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
            <div className="space-y-2">
              <Label htmlFor="title">Title *</Label>
              <Input
                id="title"
                placeholder="Video title"
                {...register("title")}
                className="h-12 bg-input"
              />
              {errors.title && (
                <p className="text-sm text-destructive">
                  {errors.title.message}
                </p>
              )}
            </div>

            <div className="space-y-2">
              <Label htmlFor="videoUrl">Video URL *</Label>
              <Input
                id="videoUrl"
                placeholder="https://youtube.com/watch?v=..."
                {...register("videoUrl")}
                className="h-12 bg-input"
              />
              {errors.videoUrl && (
                <p className="text-sm text-destructive">
                  {errors.videoUrl.message}
                </p>
              )}
            </div>

            <div className="space-y-2">
              <Label htmlFor="thumbnailUrl">Thumbnail URL</Label>
              <Input
                id="thumbnailUrl"
                placeholder="https://example.com/thumbnail.jpg"
                {...register("thumbnailUrl")}
                className="h-12 bg-input"
              />
              {errors.thumbnailUrl && (
                <p className="text-sm text-destructive">
                  {errors.thumbnailUrl.message}
                </p>
              )}
            </div>

            <div className="flex justify-end gap-3 pt-4">
              <Button
                type="button"
                variant="outline"
                onClick={() => setShowDialog(false)}
              >
                Cancel
              </Button>
              <Button type="submit" disabled={isSaving}>
                {isSaving && <LoadingSpinner size="sm" className="mr-2" />}
                {editingVideo ? "Save Changes" : "Add Video"}
              </Button>
            </div>
          </form>
        </DialogContent>
      </Dialog>

      {/* Delete Dialog */}
      <AlertDialog open={!!deleteId} onOpenChange={() => setDeleteId(null)}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>Delete Video</AlertDialogTitle>
            <AlertDialogDescription>
              Are you sure you want to delete this video? This action cannot be
              undone.
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel>Cancel</AlertDialogCancel>
            <AlertDialogAction
              onClick={handleDelete}
              className="bg-destructive text-destructive-foreground hover:bg-destructive/90"
            >
              Delete
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </div>
  );
}

// Video Card Component
interface VideoCardProps {
  video: ArtistVideo;
  onEdit: () => void;
  onDelete: () => void;
}

function VideoCard({ video, onEdit, onDelete }: VideoCardProps) {
  return (
    <div className="group rounded-xl border border-border bg-card overflow-hidden">
      {/* Thumbnail */}
      <div className="relative aspect-video bg-secondary">
        {video.thumbnailUrl ? (
          // eslint-disable-next-line @next/next/no-img-element
          <img
            src={video.thumbnailUrl}
            alt={video.title}
            className="h-full w-full object-cover"
          />
        ) : (
          <div className="flex h-full items-center justify-center">
            <Video className="h-12 w-12 text-muted-foreground" />
          </div>
        )}
        {/* Play button overlay */}
        <a
          href={video.videoUrl}
          target="_blank"
          rel="noopener noreferrer"
          className="absolute inset-0 flex items-center justify-center bg-background/50 opacity-0 transition-opacity group-hover:opacity-100"
        >
          <div className="flex h-14 w-14 items-center justify-center rounded-full bg-primary">
            <Video className="h-6 w-6 text-primary-foreground" />
          </div>
        </a>
        {/* Actions */}
        <div className="absolute right-3 top-3">
          <DropdownMenu>
            <DropdownMenuTrigger asChild>
              <Button
                variant="secondary"
                size="icon"
                className="h-8 w-8 opacity-0 transition-opacity group-hover:opacity-100"
              >
                <MoreHorizontal className="h-4 w-4" />
              </Button>
            </DropdownMenuTrigger>
            <DropdownMenuContent align="end">
              <DropdownMenuItem onClick={onEdit}>
                <Pencil className="mr-2 h-4 w-4" />
                Edit
              </DropdownMenuItem>
              <DropdownMenuItem asChild>
                <a
                  href={video.videoUrl}
                  target="_blank"
                  rel="noopener noreferrer"
                >
                  <ExternalLink className="mr-2 h-4 w-4" />
                  Open Video
                </a>
              </DropdownMenuItem>
              <DropdownMenuItem
                onClick={onDelete}
                className="text-destructive focus:text-destructive"
              >
                <Trash2 className="mr-2 h-4 w-4" />
                Delete
              </DropdownMenuItem>
            </DropdownMenuContent>
          </DropdownMenu>
        </div>
      </div>

      {/* Info */}
      <div className="p-4">
        <h3 className="truncate font-semibold text-foreground">
          {video.title}
        </h3>
        <p className="mt-1 truncate text-sm text-muted-foreground">
          {video.videoUrl}
        </p>
      </div>
    </div>
  );
}
