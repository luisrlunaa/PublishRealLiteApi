// API Types for PublishReal Backend

// Authentication
export interface RegisterDto {
  email: string;
  password: string;
}

export interface LoginDto {
  email: string;
  password: string;
}

export interface AuthResponse {
  token: string;
  email: string;
}

export interface User {
  id: string;
  email: string;
}

// Artist Profile
export interface ArtistProfile {
  id: number;
  userId: string;
  artistName: string;
  bio: string | null;
  profileImageUrl: string | null;
  socialLinksJson: string | null;
  createdAt: string;
  updatedAt: string;
}

export interface ArtistProfileDto {
  id: number;
  artistName: string;
  bio: string | null;
  profileImageUrl: string | null;
  socialLinksJson: string | null;
  isAdminProfile?: boolean;
}

export interface CreateArtistDto {
  artistName: string;
  bio?: string;
  socialLinksJson?: string;
}

export interface UpdateArtistDto {
  artistName: string;
  bio?: string;
  socialLinksJson?: string;
}

export interface CreateArtistWithAdminCodeDto {
  artistName: string;
  bio?: string;
  socialLinksJson?: string;
  adminUserId?: string;
}

export interface AdminProfileResponseDto {
  id: number;
  userId: string;
  artistName: string;
  bio: string | null;
  profileImageUrl: string | null;
  socialLinksJson: string | null;
  userIdForInvite: string;
  subProfiles: ArtistProfileDto[];
}

export interface ArtistProfileWithSubProfilesDto {
  id: number;
  artistName: string;
  bio: string | null;
  profileImageUrl: string | null;
  subProfiles: ArtistProfileDto[];
}

// Release
export interface Release {
  id: string;
  artistProfileId: number;
  title: string;
  releaseDate: string | null;
  genre: string | null;
  label: string | null;
  upc: string | null;
  isrc: string | null;
  linksJson: string | null;
  coverImageUrl: string | null;
  status: "Draft" | "Pending" | "Published";
  createdAt: string;
  updatedAt: string;
  tracks: Track[];
}

export interface ReleaseDto {
  id: string;
  artistProfileId: number;
  title: string;
  releaseDate: string | null;
  genre: string | null;
  label: string | null;
  upc: string | null;
  isrc: string | null;
  linksJson: string | null;
}

export interface CreateReleaseDto {
  artistProfileId: number;
  title: string;
  releaseDate?: string;
  genre?: string;
  label?: string;
  upc?: string;
  isrc?: string;
  linksJson?: string;
}

export interface UpdateReleaseDto {
  title: string;
  releaseDate?: string;
  genre?: string;
  label?: string;
  upc?: string;
  isrc?: string;
  linksJson?: string;
}

// Track
export interface Track {
  id: string;
  releaseId: string;
  position: number;
  title: string;
  createdAt: string;
  updatedAt: string;
}

export interface TrackDto {
  id: string;
  releaseId: string;
  position: number;
  title: string;
}

export interface CreateTrackDto {
  releaseId: string;
  position: number;
  title: string;
}

export interface UpdateTrackDto {
  position: number;
  title: string;
}

// Video
export interface ArtistVideo {
  id: number;
  artistProfileId: number;
  title: string;
  thumbnailUrl: string | null;
  videoUrl: string;
  createdAt: string;
  updatedAt: string;
}

export interface ArtistVideoDto {
  title: string;
  thumbnailUrl?: string;
  videoUrl: string;
}

// Stats
export interface StreamStatSummary {
  totalStreams: number;
  streamsByPlatform: Record<string, number>;
  streamsByCountry: Record<string, number>;
  dailyTrends: DailyTrend[];
}

export interface DailyTrend {
  date: string;
  streams: number;
}

export interface StreamStatImportDto {
  date: string;
  platform: string;
  country: string;
  streams: number;
  metricType?: string;
  source?: string;
}

// Team
export interface Team {
  id: number;
  name: string;
  artistProfileId: number;
  createdAt: string;
  members: TeamMember[];
}

export interface TeamMember {
  id: number;
  teamId: number;
  userId: string;
  email: string;
  role: string;
  joinedAt: string;
}

export interface CreateTeamRequest {
  name: string;
}

export interface InviteRequest {
  teamId: number;
  email: string;
}

// Upload
export interface UploadResult {
  url: string;
  fileName: string;
}

// API Error
export interface ApiError {
  message: string;
  errors?: string[];
  status: number;
}

// Social Links structure
export interface SocialLinks {
  spotify?: string;
  appleMusic?: string;
  instagram?: string;
  twitter?: string;
  youtube?: string;
  tiktok?: string;
  website?: string;
}

// Release Links structure
export interface ReleaseLinks {
  spotify?: string;
  appleMusic?: string;
  youtube?: string;
  soundcloud?: string;
  deezer?: string;
  tidal?: string;
}
