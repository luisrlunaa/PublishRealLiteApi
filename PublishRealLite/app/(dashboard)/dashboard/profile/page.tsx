"use client";

import { useRouter } from "next/navigation";
import { useEffect, useState } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { useAuth } from "@/lib/auth/auth-context";
import { apiClient } from "@/lib/api/client";
import type { SocialLinks, AdminProfileResponseDto, ArtistProfileDto } from "@/lib/api/types";
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
  Copy,
  Users,
  Check,
  ChevronDown,
  Pencil,
  Eye,
  Trash2,
  Plus,
} from "lucide-react";
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
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";

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
  const [adminProfile, setAdminProfile] = useState<AdminProfileResponseDto | null>(null);
  const [isLoadingAdmin, setIsLoadingAdmin] = useState(false);
  const [copiedCode, setCopiedCode] = useState(false);
  const [selectedSubProfileId, setSelectedSubProfileId] = useState<number | null>(null);
  const [isDropdownOpen, setIsDropdownOpen] = useState(false);
  const [openManageMenu, setOpenManageMenu] = useState<number | null>(null);
  const [deleteConfirm, setDeleteConfirm] = useState<number | null>(null);
  const [isDeleting, setIsDeleting] = useState(false);
  const router = useRouter();

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

  // Load admin profile if user is admin
  useEffect(() => {
    const loadAdminProfile = async () => {
      if (profile?.isAdminProfile) {
        setIsLoadingAdmin(true);
        try {
          const data = await apiClient.getMyAdminProfile();
          setAdminProfile(data);
        } catch (err) {
          console.warn("Error loading admin profile:", err);
        } finally {
          setIsLoadingAdmin(false);
        }
      } else {
        setAdminProfile(null);
      }
    };

    loadAdminProfile();
  }, [profile?.isAdminProfile]);

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
      bio: data.bio || undefined,
      socialLinksJson: JSON.stringify(socialLinks),
    };

    if (profile) {
      await apiClient.updateArtistProfile(profile.id, payload);
      setProfile({ 
        ...profile, 
        ...payload,
        bio: payload.bio || null,                    // ← CONVERT undefined to null
        socialLinksJson: payload.socialLinksJson || null,  // ← CONVERT undefined to null
        isAdminProfile: profile.isAdminProfile ?? false 
      });
      toast.success("Profile updated successfully");
    } else {
      const newProfile = await apiClient.createArtistProfile(payload);
      setProfile({ 
        ...newProfile, 
        bio: newProfile.bio || null,                 // ← CONVERT undefined to null
        socialLinksJson: newProfile.socialLinksJson || null,  // ← CONVERT undefined to null
        isAdminProfile: newProfile.isAdminProfile ?? false 
      });
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

  const handleCopyCode = () => {
    if (adminProfile?.userIdForInvite) {
      navigator.clipboard.writeText(adminProfile.userIdForInvite);
      setCopiedCode(true);
      toast.success("Invitation code copied to clipboard");
      setTimeout(() => setCopiedCode(false), 2000);
    }
  };

  const handleDeleteProfile = async (profileId: number) => {
    setIsDeleting(true);
    try {
      await apiClient.deleteArtistProfile(profileId);
      toast.success("Profile deleted successfully");
      setDeleteConfirm(null);
      // Refresh admin profile
      const data = await apiClient.getMyAdminProfile();
      setAdminProfile(data);
    } catch (err) {
      const message =
        err && typeof err === "object" && "message" in err
          ? (err as { message: string }).message
          : "Failed to delete profile";
      toast.error(message);
    } finally {
      setIsDeleting(false);
    }
  };

  const handleSelectProfile = (subProfileId: number) => {
    setSelectedSubProfileId(subProfileId);
    setIsDropdownOpen(false);
  };

  const getSelectedProfileName = () => {
    if (!selectedSubProfileId || !adminProfile) return "Select a profile";
    const selected = adminProfile.subProfiles.find(p => p.id === selectedSubProfileId);
    return selected?.artistName || "Select a profile";
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
              <Label htmlFor="artistName">Artist Name</Label>
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
              <Label htmlFor="bio">Bio</Label>
              <Textarea
                id="bio"
                placeholder="Tell us about yourself..."
                {...register("bio")}
                className="min-h-[120px] bg-input"
              />
              {errors.bio && (
                <p className="text-sm text-destructive">{errors.bio.message}</p>
              )}
            </div>
          </div>
        </div>

        {/* Social Links */}
        <div className="rounded-xl border border-border bg-card p-6">
          <div className="mb-4 flex items-center gap-2">
            <Music className="h-5 w-5 text-primary" />
            <h2 className="text-lg font-semibold text-foreground">
              Social & Music Links
            </h2>
          </div>
          <p className="mb-4 text-sm text-muted-foreground">
            Add links to your music platforms and social media
          </p>
          <div className="grid gap-4 md:grid-cols-2">
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

      {/* Admin Profile Management Section - Only for Admins */}
      {profile?.isAdminProfile && (
        <div className="space-y-6 border-t border-border pt-8">
          <div>
            <h2 className="text-2xl font-bold text-foreground">Team Management</h2>
            <p className="mt-1 text-muted-foreground">
              Manage your team members and share invitations
            </p>
          </div>

          {/* Profile Selector Dropdown */}
          {adminProfile && adminProfile.subProfiles.length > 0 && (
            <div className="rounded-xl border border-border bg-card p-6">
              <Label className="mb-3 block text-sm font-semibold">Switch Profile</Label>
              <div className="relative">
                <button
                  type="button"
                  onClick={() => setIsDropdownOpen(!isDropdownOpen)}
                  className="w-full flex items-center justify-between px-4 py-3 rounded-lg bg-input border border-border hover:bg-secondary/50 transition-colors text-foreground text-left"
                >
                  <span>{getSelectedProfileName()}</span>
                  <ChevronDown className={`h-4 w-4 transition-transform ${isDropdownOpen ? 'rotate-180' : ''}`} />
                </button>

                {isDropdownOpen && (
                  <div className="absolute top-full left-0 right-0 mt-2 bg-card border border-border rounded-lg shadow-lg z-50">
                    {adminProfile.subProfiles.map((subProfile) => (
                      <button
                        key={subProfile.id}
                        type="button"
                        onClick={() => handleSelectProfile(subProfile.id)}
                        className={`w-full text-left px-4 py-3 hover:bg-secondary/50 transition-colors first:rounded-t-lg last:rounded-b-lg ${
                          selectedSubProfileId === subProfile.id ? 'bg-primary/10 text-primary font-semibold' : ''
                        }`}
                      >
                        <div className="flex items-center gap-3">
                          <div className="flex h-6 w-6 items-center justify-center rounded-full bg-primary/10">
                            <User className="h-3 w-3 text-primary" />
                          </div>
                          <div>
                            <p className="font-medium">{subProfile.artistName}</p>
                            <p className="text-xs text-muted-foreground">ID: {subProfile.id}</p>
                          </div>
                        </div>
                      </button>
                    ))}
                  </div>
                )}
              </div>

              {selectedSubProfileId && (
                <div className="mt-4 p-3 rounded-lg bg-secondary/30 border border-border">
                  <p className="text-sm text-muted-foreground">
                    Selected: <span className="font-semibold text-foreground">{getSelectedProfileName()}</span>
                  </p>
                </div>
              )}
            </div>
          )}

          {/* Invitation Code Card */}
          <div className="rounded-xl border border-border bg-card p-6">
            <div className="flex items-center gap-2 mb-4">
              <Users className="h-5 w-5 text-primary" />
              <h3 className="text-lg font-semibold text-foreground">
                Invitation Code
              </h3>
            </div>

            <p className="text-sm text-muted-foreground mb-4">
              Share this code with team members so they can join your organization during registration.
            </p>

            {isLoadingAdmin ? (
              <LoadingSpinner />
            ) : (
              <div className="rounded-lg bg-secondary p-4">
                <Label className="text-xs font-semibold uppercase text-muted-foreground mb-2 block">
                  Your Invitation Code
                </Label>
                <div className="flex items-center gap-2">
                  <code className="flex-1 rounded bg-background px-3 py-2 font-mono text-sm break-all">
                    {adminProfile?.userIdForInvite || "Loading..."}
                  </code>
                  <Button
                    type="button"
                    variant="outline"
                    size="sm"
                    onClick={handleCopyCode}
                    disabled={isLoadingAdmin}
                  >
                    {copiedCode ? (
                      <Check className="h-4 w-4" />
                    ) : (
                      <Copy className="h-4 w-4" />
                    )}
                  </Button>
                </div>
              </div>
            )}
          </div>

          {/* Team Members Table */}
          {adminProfile && adminProfile.subProfiles.length > 0 ? (
            <div className="rounded-xl border border-border bg-card p-6">
              <div className="flex items-center justify-between mb-4">
                <h3 className="text-lg font-semibold text-foreground">
                  Team Members ({adminProfile.subProfiles.length})
                </h3>
                <Button 
                  onClick={() => router.push("/dashboard/profile/create")}
                  size="sm"
                  className="gap-2"
                >
                  <Plus className="h-4 w-4" />
                  Create Profile
                </Button>
              </div>

              <div className="overflow-x-auto">
                <table className="w-full text-sm">
                  <thead>
                    <tr className="border-b border-border">
                      <th className="px-4 py-3 text-left font-semibold text-foreground">
                        Artist Name
                      </th>
                      <th className="px-4 py-3 text-left font-semibold text-foreground">
                        Profile ID
                      </th>
                      <th className="px-4 py-3 text-left font-semibold text-foreground">
                        Actions
                      </th>
                    </tr>
                  </thead>
                  <tbody>
                    {adminProfile.subProfiles.map((subProfile) => (
                      <tr
                        key={subProfile.id}
                        className="border-b border-border hover:bg-secondary/30 transition-colors"
                      >
                        <td className="px-4 py-3">
                          <div className="flex items-center gap-3">
                            <div className="flex h-8 w-8 items-center justify-center rounded-full bg-primary/10">
                              <User className="h-4 w-4 text-primary" />
                            </div>
                            <span className="font-medium text-foreground">
                              {subProfile.artistName}
                            </span>
                          </div>
                        </td>
                        <td className="px-4 py-3 text-muted-foreground">
                          <code className="rounded bg-secondary px-2 py-1 text-xs font-mono">
                            {subProfile.id}
                          </code>
                        </td>
                        <td className="px-4 py-3">
                          <DropdownMenu open={openManageMenu === subProfile.id} onOpenChange={(open) => setOpenManageMenu(open ? subProfile.id : null)}>
                            <DropdownMenuTrigger asChild>
                              <Button variant="ghost" size="sm">
                                Manage
                              </Button>
                            </DropdownMenuTrigger>
                            <DropdownMenuContent align="end">
                              <DropdownMenuItem onClick={() => router.push(`/dashboard/profile/edit?id=${subProfile.id}`)}>
                                <Pencil className="mr-2 h-4 w-4" />
                                Edit
                              </DropdownMenuItem>
                              <DropdownMenuItem onClick={() => router.push(`/dashboard/profile/id?id=${subProfile.id}`)}>
                                <Eye className="mr-2 h-4 w-4" />
                                View Details
                              </DropdownMenuItem>
                              <DropdownMenuItem 
                                onClick={() => setDeleteConfirm(subProfile.id)}
                                className="text-destructive focus:text-destructive"
                              >
                                <Trash2 className="mr-2 h-4 w-4" />
                                Delete
                              </DropdownMenuItem>
                            </DropdownMenuContent>
                          </DropdownMenu>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </div>
          ) : (
            <div className="rounded-xl border border-dashed border-border bg-card p-6 text-center">
              <Users className="mx-auto h-12 w-12 text-muted-foreground/50 mb-2" />
              <p className="text-sm text-muted-foreground">
                No team members yet. Share your invitation code to add team members.
              </p>
            </div>
          )}
        </div>
      )}

      {/* Delete Confirmation Dialog */}
      <AlertDialog open={!!deleteConfirm} onOpenChange={() => setDeleteConfirm(null)}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>Delete Profile</AlertDialogTitle>
            <AlertDialogDescription>
              Are you sure you want to delete this artist profile? This action cannot be undone.
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel>Cancel</AlertDialogCancel>
            <AlertDialogAction
              onClick={() => deleteConfirm && handleDeleteProfile(deleteConfirm)}
              disabled={isDeleting}
              className="bg-destructive text-destructive-foreground hover:bg-destructive/90"
            >
              {isDeleting ? <LoadingSpinner size="sm" className="mr-2" /> : null}
              Delete
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
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