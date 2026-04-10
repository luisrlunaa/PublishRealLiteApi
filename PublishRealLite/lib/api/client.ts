// API Client for PublishReal Backend

import type {
  RegisterDto,
  LoginDto,
  AuthResponse,
  ArtistProfileDto,
  CreateArtistDto,
  UpdateArtistDto,
  ReleaseDto,
  CreateReleaseDto,
  UpdateReleaseDto,
  TrackDto,
  CreateTrackDto,
  UpdateTrackDto,
  ArtistVideo,
  ArtistVideoDto,
  StreamStatSummary,
  StreamStatImportDto,
  Team,
  CreateTeamRequest,
  InviteRequest,
  UploadResult,
  ApiError,
} from "./types";

const API_BASE_URL =
  process.env.NEXT_PUBLIC_API_URL || "https://localhost:44317/api";

class ApiClient {
  private baseUrl: string;

  constructor(baseUrl: string = API_BASE_URL) {
    this.baseUrl = baseUrl;
  }

  private getToken(): string | null {
    if (typeof window === "undefined") return null;
    return localStorage.getItem("publishreal_token");
  }

  private async request<T>(
    endpoint: string,
    options: RequestInit = {}
  ): Promise<T> {
    const token = this.getToken();

    const headers: HeadersInit = {
      "Content-Type": "application/json",
      ...options.headers,
    };

    if (token) {
      (headers as Record<string, string>)["Authorization"] = `Bearer ${token}`;
    }

    const response = await fetch(`${this.baseUrl}${endpoint}`, {
      ...options,
      headers,
    });

    if (!response.ok) {
      let errorMessage = "An error occurred";
      let errors: string[] | undefined;

      try {
        // First, check if the response is JSON
        const contentType = response.headers.get("content-type");
        if (contentType && contentType.includes("application/json")) {
          const errorData = await response.json();
          if (Array.isArray(errorData)) {
            errors = errorData;
            errorMessage = errorData.join(", ");
          } else if (errorData.message) {
            errorMessage = errorData.message;
          } else if (typeof errorData === "string") {
            errorMessage = errorData;
          }
        } else {
          // If it's NOT JSON (like a .NET Developer Exception HTML page), grab the text
          const errorText = await response.text();
          console.error("Raw API Error Response:", errorText); // Logs the C# crash to your browser console
          errorMessage = `Server Error (${response.status}): Check browser console for .NET Stack Trace.`;
        }
      } catch (e) {
        errorMessage = response.statusText;
      }

      const error: ApiError = {
        message: errorMessage,
        errors,
        status: response.status,
      };

      throw error;
    }

    // Handle 204 No Content
    if (response.status === 204) {
      return undefined as T;
    }

    return response.json();
  }

  // Authentication
  async register(dto: RegisterDto): Promise<{ id: string; email: string }> {
    return this.request("/Auth/register", {
      method: "POST",
      body: JSON.stringify(dto),
    });
  }

  async login(dto: LoginDto): Promise<AuthResponse> {
    const response = await this.request<AuthResponse>("/Auth/login", {
      method: "POST",
      body: JSON.stringify(dto),
    });

    if (response.token) {
      localStorage.setItem("publishreal_token", response.token);
      localStorage.setItem("publishreal_email", response.email);
    }

    return response;
  }

  logout(): void {
    if (typeof window === "undefined") return;
    localStorage.removeItem("publishreal_token");
    localStorage.removeItem("publishreal_email");
  }

  isAuthenticated(): boolean {
    return !!this.getToken();
  }

  // Artist Profiles
  async getArtistProfiles(): Promise<ArtistProfileDto[]> {
    return this.request("/ArtistProfiles");
  }

  async getArtistProfile(id: number): Promise<ArtistProfileDto> {
    return this.request(`/ArtistProfiles/${id}`);
  }

  async createArtistProfile(dto: CreateArtistDto): Promise<ArtistProfileDto> {
    return this.request("/ArtistProfiles", {
      method: "POST",
      body: JSON.stringify(dto),
    });
  }

  async updateArtistProfile(id: number, dto: UpdateArtistDto): Promise<void> {
    return this.request(`/ArtistProfiles/${id}`, {
      method: "PUT",
      body: JSON.stringify(dto),
    });
  }

  async deleteArtistProfile(id: number): Promise<void> {
    return this.request(`/ArtistProfiles/${id}`, {
      method: "DELETE",
    });
  }

  // Releases
  async getReleases(): Promise<ReleaseDto[]> {
    return this.request("/Releases");
  }

  async getRelease(id: string): Promise<ReleaseDto> {
    return this.request(`/Releases/${id}`);
  }

  async createRelease(dto: CreateReleaseDto): Promise<ReleaseDto> {
    return this.request("/Releases", {
      method: "POST",
      body: JSON.stringify(dto),
    });
  }

  async updateRelease(id: string, dto: UpdateReleaseDto): Promise<void> {
    return this.request(`/Releases/${id}`, {
      method: "PUT",
      body: JSON.stringify(dto),
    });
  }

  async deleteRelease(id: string): Promise<void> {
    return this.request(`/Releases/${id}`, {
      method: "DELETE",
    });
  }

  // Tracks
  async getTracksByRelease(releaseId: string): Promise<TrackDto[]> {
    return this.request(`/Tracks/by-release/${releaseId}`);
  }

  async createTrack(dto: CreateTrackDto): Promise<TrackDto> {
    return this.request("/Tracks", {
      method: "POST",
      body: JSON.stringify(dto),
    });
  }

  async updateTrack(id: string, dto: UpdateTrackDto): Promise<void> {
    return this.request(`/Tracks/${id}`, {
      method: "PUT",
      body: JSON.stringify(dto),
    });
  }

  async deleteTrack(id: string): Promise<void> {
    return this.request(`/Tracks/${id}`, {
      method: "DELETE",
    });
  }

  // Videos
  async getMyVideos(): Promise<ArtistVideo[]> {
    return this.request("/Videos/mine");
  }

  async createVideo(dto: ArtistVideoDto): Promise<ArtistVideo> {
    return this.request("/Videos", {
      method: "POST",
      body: JSON.stringify(dto),
    });
  }

  async updateVideo(id: number, dto: ArtistVideoDto): Promise<void> {
    return this.request(`/Videos/${id}`, {
      method: "PUT",
      body: JSON.stringify(dto),
    });
  }

  async deleteVideo(id: number): Promise<void> {
    return this.request(`/Videos/${id}`, {
      method: "DELETE",
    });
  }

  // Stats
  async getStatsSummary(range: number = 30): Promise<StreamStatSummary> {
    return this.request(`/Stats/summary?range=${range}`);
  }

  async importStats(
    items: StreamStatImportDto[]
  ): Promise<{ imported: number }> {
    return this.request("/Stats/import", {
      method: "POST",
      body: JSON.stringify(items),
    });
  }

  // Teams
  async getMyTeams(): Promise<Team[]> {
    return this.request("/Teams/mine");
  }

  async createTeam(dto: CreateTeamRequest): Promise<Team> {
    return this.request("/Teams", {
      method: "POST",
      body: JSON.stringify(dto),
    });
  }

  async inviteToTeam(dto: InviteRequest): Promise<void> {
    return this.request("/Teams/invite", {
      method: "POST",
      body: JSON.stringify(dto),
    });
  }

  async acceptTeamInvite(token: string): Promise<void> {
    return this.request(`/Teams/accept?token=${encodeURIComponent(token)}`, {
      method: "POST",
    });
  }

  // Uploads
  async uploadImage(file: File, folder: string = "covers"): Promise<UploadResult> {
    const token = this.getToken();
    const formData = new FormData();
    formData.append("file", file);

    const headers: HeadersInit = {};
    if (token) {
      headers["Authorization"] = `Bearer ${token}`;
    }

    const response = await fetch(
      `${this.baseUrl}/Uploads/image?folder=${encodeURIComponent(folder)}`,
      {
        method: "POST",
        headers,
        body: formData,
      }
    );

    if (!response.ok) {
      const error: ApiError = {
        message: "Upload failed",
        status: response.status,
      };
      throw error;
    }

    return response.json();
  }
}

export const apiClient = new ApiClient();
export { ApiClient };