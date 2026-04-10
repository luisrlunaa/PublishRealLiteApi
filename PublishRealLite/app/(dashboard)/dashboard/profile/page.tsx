"use client";

import { useEffect, useState } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { useAuth } from "@/lib/auth/auth-context";
import { apiClient } from "@/lib/api/client";
import type { SocialLinks } from "@/lib/api/types";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Textarea } from "@/components/ui/textarea";
import { LoadingSpinner, PageLoader } from "@/components/loading-states";
import { AsyncError } from "@/components/error-boundary";
import { toast } from "sonner";
import {
  User,
  Save,
  Camera,
  Instagram,
  Twitter,
  Youtube,
  Globe,
  Music,
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

export default function ProfilePage() {
  const { profile, setProfile, refreshProfile } = useAuth();
  const [isLoading, setIsLoading] = useState(true);
  const [isSaving, setIsSaving] = useState(false);
  const [error, setError] = useState<Error | null>(null);

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
        await refreshProfile();
      } catch (err) {
        setError(
          err instanceof Error ? err : new Error("Failed to load profile")
        );
      } finally {
        setIsLoading(false);
      }
    };

    loadProfile();
  }, [refreshProfile]);

  useEffect(() => {
    if (profile) {
      const socialLinks: SocialLinks = profile.socialLinksJson
        ? JSON.parse(profile.socialLinksJson)
        : {};

      reset({
        artistName: profile.artistName,
        bio: profile.bio || "",
        spotify: socialLinks.spotify || "",
        appleMusic: socialLinks.appleMusic || "",
        instagram: socialLinks.instagram || "",
        twitter: socialLinks.twitter || "",
        youtube: socialLinks.youtube || "",
        website: socialLinks.website || "",
      });
    }
  }, [profile, reset]);

  const onSubmit = async (data: ProfileFormData) => {
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
        bio: data.bio,
        socialLinksJson: JSON.stringify(socialLinks),
      };

      if (profile) {
        await apiClient.updateArtistProfile(profile.id, payload);
        setProfile({ ...profile, ...payload });
        toast.success("Profile updated successfully");
      } else {
        const newProfile = await apiClient.createArtistProfile(payload);
        setProfile(newProfile);
        toast.success("Profile created successfully");
      }
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
      <div>
        <h1 className="text-3xl font-bold text-foreground">Artist Profile</h1>
        <p className="mt-1 text-muted-foreground">
          Manage your artist identity and social links
        </p>
      </div>

      <form onSubmit={handleSubmit(onSubmit)} className="space-y-8">
        {/* Profile Image Section */}
        <div className="rounded-xl border border-border bg-card p-6">
          <h2 className="mb-4 text-lg font-semibold text-foreground">
            Profile Image
          </h2>
          <div className="flex items-center gap-6">
            <div className="relative">
              <div className="flex h-24 w-24 items-center justify-center overflow-hidden rounded-full bg-secondary">
                {profile?.profileImageUrl ? (
                  // eslint-disable-next-line @next/next/no-img-element
                  <img
                    src={profile.profileImageUrl}
                    alt={profile.artistName}
                    className="h-full w-full object-cover"
                  />
                ) : (
                  <User className="h-12 w-12 text-muted-foreground" />
                )}
              </div>
              <button
                type="button"
                className="absolute bottom-0 right-0 flex h-8 w-8 items-center justify-center rounded-full bg-primary text-primary-foreground"
              >
                <Camera className="h-4 w-4" />
              </button>
            </div>
            <div>
              <p className="text-sm text-muted-foreground">
                Upload a profile picture. Recommended size: 400x400px
              </p>
              <p className="mt-1 text-xs text-muted-foreground">
                JPG, PNG or GIF. Max 4MB.
              </p>
            </div>
          </div>
        </div>

        {/* Basic Info */}
        <div className="rounded-xl border border-border bg-card p-6">
          <h2 className="mb-4 text-lg font-semibold text-foreground">
            Basic Information
          </h2>
          <div className="space-y-4">
            <div className="space-y-2">
              <Label htmlFor="artistName">Artist Name *</Label>
              <Input
                id="artistName"
                placeholder="Your artist name"
                {...register("artistName")}
                className="h-12 bg-input"
              />
              {errors.artistName && (
                <p className="text-sm text-destructive">
                  {errors.artistName.message}
                </p>
              )}
            </div>

            <div className="space-y-2">
              <Label htmlFor="bio">Biography</Label>
              <Textarea
                id="bio"
                placeholder="Tell your story..."
                {...register("bio")}
                className="min-h-[120px] bg-input"
              />
            </div>
          </div>
        </div>

        {/* Social Links */}
        <div className="rounded-xl border border-border bg-card p-6">
          <h2 className="mb-4 text-lg font-semibold text-foreground">
            Social Links
          </h2>
          <div className="grid gap-4 sm:grid-cols-2">
            <SocialLinkInput
              id="spotify"
              label="Spotify"
              icon={<Music className="h-5 w-5" />}
              placeholder="https://open.spotify.com/artist/..."
              register={register}
              error={errors.spotify?.message}
            />
            <SocialLinkInput
              id="appleMusic"
              label="Apple Music"
              icon={<Music className="h-5 w-5" />}
              placeholder="https://music.apple.com/artist/..."
              register={register}
              error={errors.appleMusic?.message}
            />
            <SocialLinkInput
              id="instagram"
              label="Instagram"
              icon={<Instagram className="h-5 w-5" />}
              placeholder="https://instagram.com/..."
              register={register}
              error={errors.instagram?.message}
            />
            <SocialLinkInput
              id="twitter"
              label="Twitter / X"
              icon={<Twitter className="h-5 w-5" />}
              placeholder="https://twitter.com/..."
              register={register}
              error={errors.twitter?.message}
            />
            <SocialLinkInput
              id="youtube"
              label="YouTube"
              icon={<Youtube className="h-5 w-5" />}
              placeholder="https://youtube.com/@..."
              register={register}
              error={errors.youtube?.message}
            />
            <SocialLinkInput
              id="website"
              label="Website"
              icon={<Globe className="h-5 w-5" />}
              placeholder="https://yourwebsite.com"
              register={register}
              error={errors.website?.message}
            />
          </div>
        </div>

        {/* Actions */}
        <div className="flex items-center justify-end gap-4">
          <Button
            type="button"
            variant="outline"
            onClick={() => reset()}
            disabled={!isDirty || isSaving}
          >
            Cancel
          </Button>
          <Button type="submit" disabled={isSaving}>
            {isSaving && <LoadingSpinner size="sm" className="mr-2" />}
            <Save className="mr-2 h-4 w-4" />
            {profile ? "Save Changes" : "Create Profile"}
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
