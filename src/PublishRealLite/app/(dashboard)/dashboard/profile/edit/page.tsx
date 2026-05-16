"use client";

import { useEffect, useState } from "react";
import { useRouter, useSearchParams } from "next/navigation";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { apiClient } from "@/lib/api/client";
import type { ArtistProfileDto, SocialLinks } from "@/lib/api/types";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Textarea } from "@/components/ui/textarea";
import { LoadingSpinner, PageLoader } from "@/components/loading-states";
import { AsyncError } from "@/components/error-boundary";
import { toast } from "sonner";
import {
  ArrowLeft,
  Save,
  Music,
  Instagram,
  Twitter,
  Youtube,
  Globe,
} from "lucide-react";

const profileSchema = z.object({
  artistName: z.string().min(1, "Artist name is required"),
  bio: z.string().optional(),
  spotify: z.string().url().optional().or(z.literal("")),
  appleMusic: z.string().url().optional().or(z.literal("")),
  instagram: z.string().url().optional().or(z.literal("")),
  twitter: z.string().url().optional().or(z.literal("")),
  youtube: z.string().url().optional().or(z.literal("")),
  website: z.string().url().optional().or(z.literal("")),
});

type ProfileFormData = z.infer<typeof profileSchema>;

export default function EditArtistProfilePage() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const profileId = searchParams.get("id") as string;
  
  const [isLoading, setIsLoading] = useState(true);
  const [isSaving, setIsSaving] = useState(false);
  const [error, setError] = useState<Error | null>(null);
  const [profile, setProfile] = useState<ArtistProfileDto | null>(null);

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors, isDirty },
  } = useForm<ProfileFormData>({
    resolver: zodResolver(profileSchema),
  });

  useEffect(() => {
    const loadProfile = async () => {
      try {
        const data = await apiClient.getArtistProfile(parseInt(profileId));
        setProfile(data);

        const socialLinks: SocialLinks = data.socialLinksJson
          ? JSON.parse(data.socialLinksJson)
          : {};

        reset({
          artistName: data.artistName,
          bio: data.bio || "",
          spotify: socialLinks.spotify || "",
          appleMusic: socialLinks.appleMusic || "",
          instagram: socialLinks.instagram || "",
          twitter: socialLinks.twitter || "",
          youtube: socialLinks.youtube || "",
          website: socialLinks.website || "",
        });
      } catch (err) {
        setError(
          err instanceof Error ? err : new Error("Failed to load profile")
        );
      } finally {
        setIsLoading(false);
      }
    };

    loadProfile();
  }, [profileId, reset]);

  const onSubmit = async (data: ProfileFormData) => {
    if (!profile) return;
    
    setIsSaving(true);

    try {
      const socialLinks: SocialLinks = {
        spotify: data.spotify || undefined,
        appleMusic: data.appleMusic || undefined,
        instagram: data.instagram || undefined,
        twitter: data.twitter || undefined,
        youtube: data.youtube || undefined,
        website: data.website || undefined,
      };

      const payload = {
        artistName: data.artistName,
        bio: data.bio || undefined,
        socialLinksJson: JSON.stringify(socialLinks),
      };

      await apiClient.updateArtistProfile(profile.id, payload);
      toast.success("Profile updated successfully");
      router.push("/dashboard/settings/profile");
    } catch (err) {
      const message =
        err && typeof err === "object" && "message" in err
          ? (err as { message: string }).message
          : "Failed to save profile";
      toast.error(message);
    } finally {
      setIsSaving(false);
    }
  };

  if (isLoading) {
    return <PageLoader />;
  }

  if (error) {
    return (
      <AsyncError error={error} onRetry={() => window.location.reload()} />
    );
  }

  return (
    <div className="space-y-8">
      {/* Header */}
      <div className="flex items-center gap-4">
        <Button
          variant="ghost"
          size="icon"
          onClick={() => router.back()}
        >
          <ArrowLeft className="h-4 w-4" />
        </Button>
        <div>
          <h1 className="text-3xl font-bold text-foreground">Edit Artist Profile</h1>
          <p className="mt-1 text-muted-foreground">
            Update {profile?.artistName}'s information
          </p>
        </div>
      </div>

      <form onSubmit={handleSubmit(onSubmit)} className="space-y-8">
        {/* Basic Info */}
        <div className="rounded-xl border border-border bg-card p-6">
          <h2 className="mb-6 text-lg font-semibold text-foreground">
            Basic Information
          </h2>
          <div className="space-y-4">
            <div>
              <Label htmlFor="artistName">Artist Name *</Label>
              <Input
                id="artistName"
                placeholder="Your artist name"
                {...register("artistName")}
                className="mt-2 h-12 bg-input"
              />
              {errors.artistName && (
                <p className="mt-1 text-sm text-destructive">
                  {errors.artistName.message}
                </p>
              )}
            </div>

            <div>
              <Label htmlFor="bio">Bio</Label>
              <Textarea
                id="bio"
                placeholder="Tell us about yourself..."
                {...register("bio")}
                className="mt-2 min-h-24 bg-input"
              />
              {errors.bio && (
                <p className="mt-1 text-sm text-destructive">
                  {errors.bio.message}
                </p>
              )}
            </div>
          </div>
        </div>

        {/* Social Links */}
        <div className="rounded-xl border border-border bg-card p-6">
          <h2 className="mb-6 text-lg font-semibold text-foreground">
            Social Links
          </h2>
          <div className="space-y-4">
            <SocialLinkInput
              id="spotify"
              label="Spotify"
              icon={<Music className="h-4 w-4" />}
              placeholder="https://open.spotify.com/artist/..."
              register={register}
              error={errors.spotify?.message}
            />
            <SocialLinkInput
              id="appleMusic"
              label="Apple Music"
              icon={<Music className="h-4 w-4" />}
              placeholder="https://music.apple.com/artist/..."
              register={register}
              error={errors.appleMusic?.message}
            />
            <SocialLinkInput
              id="instagram"
              label="Instagram"
              icon={<Instagram className="h-4 w-4" />}
              placeholder="https://instagram.com/..."
              register={register}
              error={errors.instagram?.message}
            />
            <SocialLinkInput
              id="twitter"
              label="Twitter/X"
              icon={<Twitter className="h-4 w-4" />}
              placeholder="https://twitter.com/..."
              register={register}
              error={errors.twitter?.message}
            />
            <SocialLinkInput
              id="youtube"
              label="YouTube"
              icon={<Youtube className="h-4 w-4" />}
              placeholder="https://youtube.com/@..."
              register={register}
              error={errors.youtube?.message}
            />
            <SocialLinkInput
              id="website"
              label="Personal Website"
              icon={<Globe className="h-4 w-4" />}
              placeholder="https://yourwebsite.com"
              register={register}
              error={errors.website?.message}
            />
          </div>
        </div>

        {/* Submit Button */}
        <div className="flex gap-3">
          <Button
            type="button"
            variant="outline"
            onClick={() => router.back()}
          >
            Cancel
          </Button>
          <Button type="submit" disabled={isSaving || !isDirty}>
            {isSaving && <LoadingSpinner size="sm" className="mr-2" />}
            <Save className="mr-2 h-4 w-4" />
            Save Changes
          </Button>
        </div>
      </form>
    </div>
  );
}

// Social Link Input Component
interface SocialLinkInputProps {
  id: keyof ProfileFormData;
  label: string;
  icon: React.ReactNode;
  placeholder: string;
  register: ReturnType<typeof useForm<ProfileFormData>>["register"];
  error?: string;
}

function SocialLinkInput({
  id,
  label,
  icon,
  placeholder,
  register,
  error,
}: SocialLinkInputProps) {
  return (
    <div className="space-y-2">
      <Label htmlFor={id}>{label}</Label>
      <div className="relative">
        <div className="pointer-events-none absolute left-3 top-1/2 -translate-y-1/2 text-muted-foreground">
          {icon}
        </div>
        <Input
          id={id}
          placeholder={placeholder}
          {...register(id)}
          className="h-12 bg-input pl-11"
        />
      </div>
      {error && <p className="text-sm text-destructive">{error}</p>}
    </div>
  );
}