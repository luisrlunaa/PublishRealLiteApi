"use client";

import { useEffect, useState } from "react";
import { useParams, useRouter } from "next/navigation";
import Link from "next/link";
import { apiClient } from "@/lib/api/client";
import { Button } from "@/components/ui/button";
import { PageLoader, LoadingSpinner } from "@/components/loading-states";
import { Music2, Users, CheckCircle, XCircle, LogIn } from "lucide-react";

type InviteStatus = "loading" | "success" | "error" | "unauthenticated";

export default function AcceptInvitePage() {
  const params = useParams();
  const router = useRouter();
  const token = params.token as string;

  const [status, setStatus] = useState<InviteStatus>("loading");
  const [errorMessage, setErrorMessage] = useState<string>("");

  useEffect(() => {
    const acceptInvite = async () => {
      // Check if user is authenticated
      if (!apiClient.isAuthenticated()) {
        setStatus("unauthenticated");
        return;
      }

      try {
        await apiClient.acceptTeamInvite(token);
        setStatus("success");
      } catch (err) {
        setStatus("error");
        if (err && typeof err === "object" && "message" in err) {
          setErrorMessage((err as { message: string }).message);
        } else {
          setErrorMessage("Failed to accept invitation. The invite may have expired or already been used.");
        }
      }
    };

    acceptInvite();
  }, [token]);

  if (status === "loading") {
    return <PageLoader />;
  }

  return (
    <div className="flex min-h-screen flex-col items-center justify-center bg-background px-4">
      <div className="w-full max-w-md">
        {/* Logo */}
        <div className="mb-8 text-center">
          <Link href="/" className="inline-flex items-center gap-2">
            <div className="flex h-12 w-12 items-center justify-center rounded-lg bg-primary">
              <Music2 className="h-6 w-6 text-primary-foreground" />
            </div>
            <span className="text-2xl font-bold tracking-tight text-foreground">
              PublishReal
            </span>
          </Link>
        </div>

        {/* Card */}
        <div className="rounded-xl border border-border bg-card p-8">
          {status === "unauthenticated" && (
            <div className="text-center">
              <div className="mx-auto mb-4 flex h-16 w-16 items-center justify-center rounded-full bg-primary/10">
                <Users className="h-8 w-8 text-primary" />
              </div>
              <h1 className="text-2xl font-bold text-foreground">
                Team Invitation
              </h1>
              <p className="mt-2 text-muted-foreground">
                You need to sign in to accept this team invitation.
              </p>

              <div className="mt-6 space-y-3">
                <Link
                  href={`/login?redirect=/invite/${token}`}
                  className="block"
                >
                  <Button className="w-full">
                    <LogIn className="mr-2 h-4 w-4" />
                    Sign In to Accept
                  </Button>
                </Link>
                <p className="text-sm text-muted-foreground">
                  {"Don't have an account? "}
                  <Link
                    href={`/register?redirect=/invite/${token}`}
                    className="font-semibold text-primary hover:underline"
                  >
                    Sign up
                  </Link>
                </p>
              </div>
            </div>
          )}

          {status === "success" && (
            <div className="text-center">
              <div className="mx-auto mb-4 flex h-16 w-16 items-center justify-center rounded-full bg-emerald-500/10">
                <CheckCircle className="h-8 w-8 text-emerald-500" />
              </div>
              <h1 className="text-2xl font-bold text-foreground">
                Invitation Accepted!
              </h1>
              <p className="mt-2 text-muted-foreground">
                You have successfully joined the team. You can now collaborate
                with your team members.
              </p>

              <div className="mt-6">
                <Button onClick={() => router.push("/dashboard/team")}>
                  <Users className="mr-2 h-4 w-4" />
                  Go to Team
                </Button>
              </div>
            </div>
          )}

          {status === "error" && (
            <div className="text-center">
              <div className="mx-auto mb-4 flex h-16 w-16 items-center justify-center rounded-full bg-destructive/10">
                <XCircle className="h-8 w-8 text-destructive" />
              </div>
              <h1 className="text-2xl font-bold text-foreground">
                Invitation Failed
              </h1>
              <p className="mt-2 text-muted-foreground">
                {errorMessage ||
                  "Unable to accept this invitation. Please check that the link is valid."}
              </p>

              <div className="mt-6 space-y-3">
                <Button
                  variant="outline"
                  onClick={() => window.location.reload()}
                >
                  <LoadingSpinner size="sm" className="mr-2" />
                  Try Again
                </Button>
                <Link href="/dashboard" className="block">
                  <Button variant="ghost" className="w-full">
                    Go to Dashboard
                  </Button>
                </Link>
              </div>
            </div>
          )}
        </div>

        {/* Footer */}
        <p className="mt-8 text-center text-sm text-muted-foreground">
          Having trouble?{" "}
          <Link href="#" className="text-primary hover:underline">
            Contact support
          </Link>
        </p>
      </div>
    </div>
  );
}
