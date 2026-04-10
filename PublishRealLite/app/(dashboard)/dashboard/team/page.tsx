"use client";

import { useEffect, useState } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { apiClient } from "@/lib/api/client";
import type { Team } from "@/lib/api/types";
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
  Users,
  UserPlus,
  MoreHorizontal,
  Mail,
  Crown,
  User,
} from "lucide-react";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogDescription,
} from "@/components/ui/dialog";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";

const teamSchema = z.object({
  name: z.string().min(1, "Team name is required"),
});

const inviteSchema = z.object({
  email: z.string().email("Please enter a valid email"),
});

type TeamFormData = z.infer<typeof teamSchema>;
type InviteFormData = z.infer<typeof inviteSchema>;

export default function TeamPage() {
  const [teams, setTeams] = useState<Team[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<Error | null>(null);
  const [showCreateDialog, setShowCreateDialog] = useState(false);
  const [showInviteDialog, setShowInviteDialog] = useState(false);
  const [selectedTeamId, setSelectedTeamId] = useState<number | null>(null);
  const [isSaving, setIsSaving] = useState(false);

  const teamForm = useForm<TeamFormData>({
    resolver: zodResolver(teamSchema),
  });

  const inviteForm = useForm<InviteFormData>({
    resolver: zodResolver(inviteSchema),
  });

  useEffect(() => {
    fetchTeams();
  }, []);

  const fetchTeams = async () => {
    try {
      const data = await apiClient.getMyTeams();
      setTeams(data);
    } catch (err) {
      setError(
        err instanceof Error ? err : new Error("Failed to load teams")
      );
    } finally {
      setIsLoading(false);
    }
  };

  const onCreateTeam = async (data: TeamFormData) => {
    setIsSaving(true);

    try {
      const newTeam = await apiClient.createTeam({ name: data.name });
      setTeams((prev) => [...prev, newTeam]);
      toast.success("Team created successfully");
      setShowCreateDialog(false);
      teamForm.reset();
    } catch (err) {
      const message =
        err && typeof err === "object" && "message" in err
          ? (err as { message: string }).message
          : "Failed to create team";
      toast.error(message);
    } finally {
      setIsSaving(false);
    }
  };

  const onInvite = async (data: InviteFormData) => {
    if (!selectedTeamId) return;

    setIsSaving(true);

    try {
      await apiClient.inviteToTeam({
        teamId: selectedTeamId,
        email: data.email,
      });
      toast.success("Invitation sent successfully");
      setShowInviteDialog(false);
      inviteForm.reset();
    } catch (err) {
      const message =
        err && typeof err === "object" && "message" in err
          ? (err as { message: string }).message
          : "Failed to send invitation";
      toast.error(message);
    } finally {
      setIsSaving(false);
    }
  };

  const openInviteDialog = (teamId: number) => {
    setSelectedTeamId(teamId);
    inviteForm.reset();
    setShowInviteDialog(true);
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
          <h1 className="text-3xl font-bold text-foreground">Team</h1>
          <p className="mt-1 text-muted-foreground">
            Manage your team and collaborators
          </p>
        </div>
        <Button
          className="font-semibold"
          onClick={() => setShowCreateDialog(true)}
        >
          <Plus className="mr-2 h-4 w-4" />
          Create Team
        </Button>
      </div>

      {/* Teams List */}
      {isLoading ? (
        <SkeletonGrid count={2} columns={2} />
      ) : teams.length === 0 ? (
        <EmptyState
          icon={<Users className="h-8 w-8 text-muted-foreground" />}
          title="No teams yet"
          description="Create a team to collaborate with managers, producers, and other team members."
          action={
            <Button onClick={() => setShowCreateDialog(true)}>
              <Plus className="mr-2 h-4 w-4" />
              Create Team
            </Button>
          }
        />
      ) : (
        <div className="space-y-6">
          {teams.map((team) => (
            <TeamCard
              key={team.id}
              team={team}
              onInvite={() => openInviteDialog(team.id)}
            />
          ))}
        </div>
      )}

      {/* Create Team Dialog */}
      <Dialog open={showCreateDialog} onOpenChange={setShowCreateDialog}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Create Team</DialogTitle>
            <DialogDescription>
              Create a new team to collaborate with others on your music.
            </DialogDescription>
          </DialogHeader>
          <form
            onSubmit={teamForm.handleSubmit(onCreateTeam)}
            className="space-y-4"
          >
            <div className="space-y-2">
              <Label htmlFor="teamName">Team Name *</Label>
              <Input
                id="teamName"
                placeholder="e.g., My Production Team"
                {...teamForm.register("name")}
                className="h-12 bg-input"
              />
              {teamForm.formState.errors.name && (
                <p className="text-sm text-destructive">
                  {teamForm.formState.errors.name.message}
                </p>
              )}
            </div>

            <div className="flex justify-end gap-3 pt-4">
              <Button
                type="button"
                variant="outline"
                onClick={() => setShowCreateDialog(false)}
              >
                Cancel
              </Button>
              <Button type="submit" disabled={isSaving}>
                {isSaving && <LoadingSpinner size="sm" className="mr-2" />}
                Create Team
              </Button>
            </div>
          </form>
        </DialogContent>
      </Dialog>

      {/* Invite Dialog */}
      <Dialog open={showInviteDialog} onOpenChange={setShowInviteDialog}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Invite Team Member</DialogTitle>
            <DialogDescription>
              Send an invitation to collaborate on your music.
            </DialogDescription>
          </DialogHeader>
          <form
            onSubmit={inviteForm.handleSubmit(onInvite)}
            className="space-y-4"
          >
            <div className="space-y-2">
              <Label htmlFor="inviteEmail">Email Address *</Label>
              <Input
                id="inviteEmail"
                type="email"
                placeholder="collaborator@example.com"
                {...inviteForm.register("email")}
                className="h-12 bg-input"
              />
              {inviteForm.formState.errors.email && (
                <p className="text-sm text-destructive">
                  {inviteForm.formState.errors.email.message}
                </p>
              )}
            </div>

            <div className="flex justify-end gap-3 pt-4">
              <Button
                type="button"
                variant="outline"
                onClick={() => setShowInviteDialog(false)}
              >
                Cancel
              </Button>
              <Button type="submit" disabled={isSaving}>
                {isSaving && <LoadingSpinner size="sm" className="mr-2" />}
                <Mail className="mr-2 h-4 w-4" />
                Send Invitation
              </Button>
            </div>
          </form>
        </DialogContent>
      </Dialog>
    </div>
  );
}

// Team Card Component
interface TeamCardProps {
  team: Team;
  onInvite: () => void;
}

function TeamCard({ team, onInvite }: TeamCardProps) {
  const members = team.members || [];

  return (
    <div className="rounded-xl border border-border bg-card">
      {/* Header */}
      <div className="flex items-center justify-between border-b border-border p-4">
        <div className="flex items-center gap-3">
          <div className="flex h-10 w-10 items-center justify-center rounded-lg bg-primary">
            <Users className="h-5 w-5 text-primary-foreground" />
          </div>
          <div>
            <h3 className="font-semibold text-foreground">{team.name}</h3>
            <p className="text-sm text-muted-foreground">
              {members.length} member{members.length !== 1 ? "s" : ""}
            </p>
          </div>
        </div>
        <div className="flex items-center gap-2">
          <Button variant="outline" size="sm" onClick={onInvite}>
            <UserPlus className="mr-2 h-4 w-4" />
            Invite
          </Button>
          <DropdownMenu>
            <DropdownMenuTrigger asChild>
              <Button variant="ghost" size="icon">
                <MoreHorizontal className="h-4 w-4" />
              </Button>
            </DropdownMenuTrigger>
            <DropdownMenuContent align="end">
              <DropdownMenuItem onClick={onInvite}>
                <UserPlus className="mr-2 h-4 w-4" />
                Invite Member
              </DropdownMenuItem>
            </DropdownMenuContent>
          </DropdownMenu>
        </div>
      </div>

      {/* Members */}
      <div className="p-4">
        {members.length === 0 ? (
          <div className="py-4 text-center text-muted-foreground">
            <p>No members yet.</p>
            <Button variant="link" className="mt-2" onClick={onInvite}>
              Invite your first team member
            </Button>
          </div>
        ) : (
          <div className="space-y-3">
            {members.map((member) => (
              <div
                key={member.id}
                className="flex items-center justify-between rounded-lg bg-secondary/50 p-3"
              >
                <div className="flex items-center gap-3">
                  <div className="flex h-10 w-10 items-center justify-center rounded-full bg-secondary">
                    <User className="h-5 w-5 text-muted-foreground" />
                  </div>
                  <div>
                    <p className="font-medium text-foreground">
                      {member.email}
                    </p>
                    <p className="text-sm text-muted-foreground">
                      Joined{" "}
                      {new Date(member.joinedAt).toLocaleDateString()}
                    </p>
                  </div>
                </div>
                <div className="flex items-center gap-2">
                  {member.role === "owner" && (
                    <span className="flex items-center gap-1 rounded-full bg-primary/20 px-2 py-1 text-xs font-medium text-primary">
                      <Crown className="h-3 w-3" />
                      Owner
                    </span>
                  )}
                  {member.role === "admin" && (
                    <span className="rounded-full bg-secondary px-2 py-1 text-xs font-medium text-foreground">
                      Admin
                    </span>
                  )}
                  {member.role === "member" && (
                    <span className="rounded-full bg-secondary px-2 py-1 text-xs font-medium text-muted-foreground">
                      Member
                    </span>
                  )}
                </div>
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  );
}
