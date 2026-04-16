"use client";

import { useState } from "react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { useAuth } from "@/lib/auth/auth-context";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { LoadingSpinner } from "@/components/loading-states";
import { Eye, EyeOff, Music2, Check } from "lucide-react";

const registerSchema = z
  .object({
    email: z.string().email("Please enter a valid email address"),
    password: z.string().min(6, "Password must be at least 6 characters"),
    confirmPassword: z.string().min(6, "Please confirm your password"),
    adminCode: z.string().optional().or(z.literal("")),
  })
  .refine((data) => data.password === data.confirmPassword, {
    message: "Passwords do not match",
    path: ["confirmPassword"],
  });

type RegisterFormData = z.infer<typeof registerSchema>;

export default function RegisterPage() {
  const router = useRouter();
  const { register: registerUser } = useAuth();
  const [isLoading, setIsLoading] = useState(false);
  const [showPassword, setShowPassword] = useState(false);
  const [showConfirmPassword, setShowConfirmPassword] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [useAdminCode, setUseAdminCode] = useState(false);

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<RegisterFormData>({
    resolver: zodResolver(registerSchema),
  });

  const onSubmit = async (data: RegisterFormData) => {
    setIsLoading(true);
    setError(null);

    try {
      // If admin code is provided, create profile with admin code after registration
      if (data.adminCode?.trim()) {
        // First register the user
        const registerResponse = await registerUser({ email: data.email, password: data.password });

        // Then create profile with admin code
        try {
          const profilePath = `/api/artist-profiles/with-admin-code`;
          await fetch(`${process.env.NEXT_PUBLIC_API_URL || 'https://localhost:44317'}${profilePath}`, {
            method: 'POST',
            headers: {
              'Content-Type': 'application/json',
              'Authorization': `Bearer ${localStorage.getItem('publishreal_token')}`,
            },
            body: JSON.stringify({ adminUserId: data.adminCode.trim() }),
          });
        } catch (profileErr) {
          console.warn("Profile creation with admin code succeeded but post-processing failed:", profileErr);
        }
      } else {
        // Standard registration without admin code
        await registerUser({ email: data.email, password: data.password });
      }

      router.push("/dashboard");
    } catch (err) {
      if (err && typeof err === "object" && "message" in err) {
        setError((err as { message: string }).message);
      } else {
        setError("Registration failed. Please try again.");
      }
    } finally {
      setIsLoading(false);
    }
  };

  const features = [
    "Unlimited music uploads",
    "Keep 100% of your earnings",
    "Real-time streaming statistics",
    "Team collaboration tools",
  ];

  return (
    <div className="flex min-h-screen bg-background">
      {/* Left side - Form */}
      <div className="flex flex-1 flex-col items-center justify-center px-4 py-12">
        <div className="w-full max-w-md">
          {/* Logo */}
          <div className="mb-8">
            <Link href="/" className="inline-flex items-center gap-2">
              <div className="flex h-12 w-12 items-center justify-center rounded-lg bg-primary">
                <Music2 className="h-6 w-6 text-primary-foreground" />
              </div>
              <span className="text-2xl font-bold tracking-tight text-foreground">
                PublishReal
              </span>
            </Link>
          </div>

          {/* Form Card */}
          <div className="rounded-xl border border-border bg-card p-8">
            <div className="mb-6">
              <h1 className="text-2xl font-bold text-foreground">
                Create your account
              </h1>
              <p className="mt-2 text-muted-foreground">
                Start sharing your music with the world
              </p>
            </div>

            {error && (
              <div className="mb-6 rounded-lg border border-destructive/50 bg-destructive/10 p-4">
                <p className="text-sm text-destructive">{error}</p>
              </div>
            )}

            <form onSubmit={handleSubmit(onSubmit)} className="space-y-5">
              <div className="space-y-2">
                <Label htmlFor="email">Email</Label>
                <Input
                  id="email"
                  type="email"
                  placeholder="you@example.com"
                  {...register("email")}
                  className="h-12 bg-input"
                  autoComplete="email"
                />
                {errors.email && (
                  <p className="text-sm text-destructive">
                    {errors.email.message}
                  </p>
                )}
              </div>

              <div className="space-y-2">
                <Label htmlFor="password">Password</Label>
                <div className="relative">
                  <Input
                    id="password"
                    type={showPassword ? "text" : "password"}
                    placeholder="Create a password"
                    {...register("password")}
                    className="h-12 bg-input pr-10"
                    autoComplete="new-password"
                  />
                  <button
                    type="button"
                    onClick={() => setShowPassword(!showPassword)}
                    className="absolute right-3 top-1/2 -translate-y-1/2 text-muted-foreground hover:text-foreground"
                  >
                    {showPassword ? (
                      <EyeOff className="h-5 w-5" />
                    ) : (
                      <Eye className="h-5 w-5" />
                    )}
                  </button>
                </div>
                {errors.password && (
                  <p className="text-sm text-destructive">
                    {errors.password.message}
                  </p>
                )}
              </div>

              <div className="space-y-2">
                <Label htmlFor="confirmPassword">Confirm Password</Label>
                <div className="relative">
                  <Input
                    id="confirmPassword"
                    type={showConfirmPassword ? "text" : "password"}
                    placeholder="Confirm your password"
                    {...register("confirmPassword")}
                    className="h-12 bg-input pr-10"
                    autoComplete="new-password"
                  />
                  <button
                    type="button"
                    onClick={() => setShowConfirmPassword(!showConfirmPassword)}
                    className="absolute right-3 top-1/2 -translate-y-1/2 text-muted-foreground hover:text-foreground"
                  >
                    {showConfirmPassword ? (
                      <EyeOff className="h-5 w-5" />
                    ) : (
                      <Eye className="h-5 w-5" />
                    )}
                  </button>
                </div>
                {errors.confirmPassword && (
                  <p className="text-sm text-destructive">
                    {errors.confirmPassword.message}
                  </p>
                )}
              </div>

              {/* Admin Code Section */}
              <div className="rounded-lg border border-border/50 bg-secondary/30 p-3">
                <div className="flex items-center gap-2">
                  <input
                    type="checkbox"
                    id="useAdminCode"
                    checked={useAdminCode}
                    onChange={(e) => setUseAdminCode(e.target.checked)}
                    className="h-4 w-4 rounded border-border"
                  />
                  <Label htmlFor="useAdminCode" className="text-sm font-medium cursor-pointer">
                    I'm joining as a team member under an admin
                  </Label>
                </div>
              </div>

              {useAdminCode && (
                <div className="space-y-2">
                  <Label htmlFor="adminCode">Admin Invitation Code</Label>
                  <Input
                    id="adminCode"
                    placeholder="Paste the admin's invitation code here"
                    {...register("adminCode")}
                    className="h-12 bg-input font-mono text-sm"
                  />
                  <p className="text-xs text-muted-foreground">
                    Ask your admin for their user ID to get started
                  </p>
                </div>
              )}

              <Button
                type="submit"
                className="h-12 w-full text-base font-semibold"
                disabled={isLoading}
              >
                {isLoading ? (
                  <LoadingSpinner size="sm" className="mr-2" />
                ) : null}
                {isLoading ? "Creating account..." : "Create account"}
              </Button>
            </form>

            <div className="mt-6 text-center">
              <p className="text-muted-foreground">
                Already have an account?{" "}
                <Link
                  href="/login"
                  className="font-semibold text-primary hover:underline"
                >
                  Sign in
                </Link>
              </p>
            </div>
          </div>
        </div>
      </div>

      {/* Right side - Features */}
      <div className="hidden flex-1 items-center justify-center bg-secondary/50 lg:flex">
        <div className="max-w-md px-8">
          <h2 className="text-3xl font-bold text-foreground">
            Everything you need to grow your music career
          </h2>
          <p className="mt-4 text-lg text-muted-foreground">
            Join thousands of independent artists who trust PublishReal to
            manage their music.
          </p>
          <ul className="mt-8 space-y-4">
            {features.map((feature) => (
              <li key={feature} className="flex items-center gap-3">
                <div className="flex h-6 w-6 items-center justify-center rounded-full bg-primary">
                  <Check className="h-4 w-4 text-primary-foreground" />
                </div>
                <span className="text-foreground">{feature}</span>
              </li>
            ))}
          </ul>
        </div>
      </div>
    </div>
  );
}
