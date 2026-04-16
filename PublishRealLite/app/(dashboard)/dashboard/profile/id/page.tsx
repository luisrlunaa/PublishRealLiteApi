"use client";

import { useEffect, useState } from "react";
import { useSearchParams, useRouter } from "next/navigation"; // Added useRouter
import { apiClient } from "@/lib/api/client"; 
import type { ArtistProfileDto } from "@/lib/api/types";
import { LoadingSpinner } from "@/components/loading-states";
import { Button } from "@/components/ui/button"; // Ensure you have your Button component
import { ArrowLeft, Settings } from "lucide-react"; // Icons for the buttons

export default function ProfilePage() {
  const searchParams = useSearchParams();
  const router = useRouter(); // Initialize the router
  const [profile, setProfile] = useState<ArtistProfileDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

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

        <Button 
          variant="default" 
          onClick={() => router.push(`/dashboard/profile?id=${profile.id}`)} 
          className="flex items-center gap-2"
        >
          <Settings className="h-4 w-4" />
          Manage Profile
        </Button>
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
        <div className="bg-card shadow-sm border rounded-xl p-6">
          <h2 className="text-lg font-semibold mb-4 border-b pb-2">Biography</h2>
          <p className="text-card-foreground leading-relaxed whitespace-pre-wrap">
            {profile.bio || "This artist has not provided a biography yet."}
          </p>
        </div>

        <div className="bg-muted/30 border rounded-xl p-6">
          <h2 className="text-lg font-semibold mb-4 border-b pb-2">Account Metadata</h2>
          <div className="grid grid-cols-2 gap-4 text-sm">
            <div>
              <p className="text-muted-foreground">Internal Database ID</p>
              <p className="font-mono font-medium">{profile.id}</p>
            </div>
            <div>
              <p className="text-muted-foreground">Admin Status</p>
              <p className="font-medium">{profile.isAdminProfile ? "Yes" : "No"}</p>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}