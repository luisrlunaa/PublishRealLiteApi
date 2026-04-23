"use client";

import { useEffect, useState } from "react";
import { useSearchParams, useRouter } from "next/navigation";
import { apiClient } from "@/lib/api/client";
import type { ArtistProfileDto, SocialLinks } from "@/lib/api/types";
import { LoadingSpinner } from "@/components/loading-states";
import { Button } from "@/components/ui/button";
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
import { toast } from "sonner";
import {
  ArrowLeft,
  MoreVertical,
  Pencil,
  Trash2,
  Music,
  Instagram,
  Twitter,
  Youtube,
  Globe,
} from "lucide-react";

export default function ProfilePage() {
  const searchParams = useSearchParams();
  const router = useRouter();
  const [profile, setProfile] = useState<ArtistProfileDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [deleteConfirm, setDeleteConfirm] = useState(false);
  const [isDeleting, setIsDeleting] = useState(false);
  const [socialLinks, setSocialLinks] = useState<SocialLinks>({});

  const idFromQuery = searchParams.get("id");
  const profileId = idFromQuery ? parseInt(idFromQuery) : NaN;

  useEffect(() => {
    const loadProfile = async () => {
      if (isNaN(profileId)) {
        setError("No valid Profile ID provided in the URL.");
        setLoading(false);
        return;
      }

      try {
        setLoading(true);
        const data = await apiClient.getArtistProfile(profileId);
        setProfile(data);
        
        // Parse social links
        if (data.socialLinksJson) {
          try {
            setSocialLinks(JSON.parse(data.socialLinksJson));
          } catch {
            setSocialLinks({});
          }
        }
      } catch (err: any) {
        console.error("Error fetching profile:", err);
        if (err?.status === 404) {
          setError("Profile not found in database.");
        } else {
          setError("Could not connect to the server.");
        }
      } finally {
        setLoading(false);
      }
    };

    loadProfile();
  }, [profileId]);

  const handleDelete = async () => {
    if (!profile) return;
    
    setIsDeleting(true);
    try {
      await apiClient.deleteArtistProfile(profile.id);
      toast.success("Profile deleted successfully");
      router.push("/dashboard/settings/profile");
    } catch (err: any) {
      const message = err?.message || "Failed to delete profile";
      toast.error(message);
    } finally {
      setIsDeleting(false);
      setDeleteConfirm(false);
    }
  };

  if (loading) {
    return (
      <div className="flex flex-col items-center justify-center h-64 gap-4">
        <LoadingSpinner />
        <p className="text-muted-foreground">Fetching from database...</p>
      </div>
    );
  }

  if (error || !profile) {
    return (
      <div className="flex justify-center items-center h-64">
        <div className="text-center text-red-500 bg-red-50 p-6 rounded-lg border border-red-200">
          <h2 className="text-xl font-bold mb-2">Error</h2>
          <p>{error || "Profile data unavailable"}</p>
          <Button variant="outline" className="mt-4" onClick={() => router.back()}>
            Go Back
          </Button>
        </div>
      </div>
    );
  }

  return (
    <div className="max-w-4xl mx-auto p-6 animate-in fade-in duration-500">
      {/* Navigation Header */}
      <div className="flex justify-between items-center mb-8">
        <Button 
          variant="ghost" 
          onClick={() => router.back()} 
          className="flex items-center gap-2"
        >
          <ArrowLeft className="h-4 w-4" />
          Back
        </Button>

        <DropdownMenu>
          <DropdownMenuTrigger asChild>
            <Button variant="default" size="icon">
              <MoreVertical className="h-4 w-4" />
            </Button>
          </DropdownMenuTrigger>
          <DropdownMenuContent align="end">
            <DropdownMenuItem onClick={() => router.push(`/dashboard/profile/edit?id=${profile.id}`)}>
              <Pencil className="mr-2 h-4 w-4" />
              Edit Profile
            </DropdownMenuItem>
            <DropdownMenuItem 
              onClick={() => setDeleteConfirm(true)}
              className="text-destructive focus:text-destructive"
            >
              <Trash2 className="mr-2 h-4 w-4" />
              Delete Profile
            </DropdownMenuItem>
          </DropdownMenuContent>
        </DropdownMenu>
      </div>

      <div className="flex items-center gap-6 mb-8">
        <div className="w-20 h-20 bg-primary rounded-full flex items-center justify-center text-primary-foreground text-3xl font-bold shadow-lg">
          {profile.artistName.charAt(0)}
        </div>
        <div>
          <h1 className="text-4xl font-extrabold tracking-tight text-foreground">
            {profile.artistName}
          </h1>
          <p className="text-muted-foreground">
            {profile.isAdminProfile ? "Primary Artist Account" : "Artist Profile"}
          </p>
        </div>
      </div>
      
      <div className="grid gap-6">
        {/* Biography */}
        <div className="bg-card shadow-sm border rounded-xl p-6">
          <h2 className="text-lg font-semibold mb-4 border-b pb-2">Biography</h2>
          <p className="text-card-foreground leading-relaxed whitespace-pre-wrap">
            {profile.bio || "This artist has not provided a biography yet."}
          </p>
        </div>

        {/* Social Links */}
        {Object.values(socialLinks).some(link => link) && (
          <div className="bg-card shadow-sm border rounded-xl p-6">
            <h2 className="text-lg font-semibold mb-4 border-b pb-2">Social Links</h2>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              {socialLinks.spotify && (
                <SocialLinkCard
                  icon={<Music className="h-5 w-5" />}
                  label="Spotify"
                  url={socialLinks.spotify}
                  color="text-green-500"
                />
              )}
              {socialLinks.appleMusic && (
                <SocialLinkCard
                  icon={<Music className="h-5 w-5" />}
                  label="Apple Music"
                  url={socialLinks.appleMusic}
                  color="text-pink-500"
                />
              )}
              {socialLinks.instagram && (
                <SocialLinkCard
                  icon={<Instagram className="h-5 w-5" />}
                  label="Instagram"
                  url={socialLinks.instagram}
                  color="text-pink-600"
                />
              )}
              {socialLinks.twitter && (
                <SocialLinkCard
                  icon={<Twitter className="h-5 w-5" />}
                  label="Twitter"
                  url={socialLinks.twitter}
                  color="text-blue-400"
                />
              )}
              {socialLinks.youtube && (
                <SocialLinkCard
                  icon={<Youtube className="h-5 w-5" />}
                  label="YouTube"
                  url={socialLinks.youtube}
                  color="text-red-500"
                />
              )}
              {socialLinks.website && (
                <SocialLinkCard
                  icon={<Globe className="h-5 w-5" />}
                  label="Website"
                  url={socialLinks.website}
                  color="text-blue-500"
                />
              )}
            </div>
          </div>
        )}

        {/* Account Metadata */}
        <div className="bg-muted/30 border rounded-xl p-6">
          <h2 className="text-lg font-semibold mb-4 border-b pb-2">Account Information</h2>
          <div className="grid grid-cols-2 md:grid-cols-3 gap-4 text-sm">
            <div>
              <p className="text-muted-foreground mb-1">Profile ID</p>
              <p className="font-mono font-medium text-foreground">{profile.id}</p>
            </div>
            <div>
              <p className="text-muted-foreground mb-1">Admin Profile</p>
              <p className="font-medium text-foreground">{profile.isAdminProfile ? "Yes" : "No"}</p>
            </div>
          </div>
        </div>
      </div>

      {/* Delete Confirmation Dialog */}
      <AlertDialog open={deleteConfirm} onOpenChange={setDeleteConfirm}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>Delete Profile</AlertDialogTitle>
            <AlertDialogDescription>
              Are you sure you want to delete {profile.artistName}'s profile? This action cannot be undone.
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel>Cancel</AlertDialogCancel>
            <AlertDialogAction
              onClick={handleDelete}
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

// Social Link Card Component
interface SocialLinkCardProps {
  icon: React.ReactNode;
  label: string;
  url: string;
  color: string;
}

function SocialLinkCard({ icon, label, url, color }: SocialLinkCardProps) {
  return (
    <a
      href={url}
      target="_blank"
      rel="noopener noreferrer"
      className="flex items-center gap-3 p-4 rounded-lg border border-border hover:bg-secondary/50 transition-colors group"
    >
      <div className={`${color}`}>
        {icon}
      </div>
      <div className="flex-1 min-w-0">
        <p className="text-sm font-medium text-muted-foreground">{label}</p>
        <p className="text-sm text-foreground truncate group-hover:underline">
          {new URL(url).hostname}
        </p>
      </div>
    </a>
  );
}