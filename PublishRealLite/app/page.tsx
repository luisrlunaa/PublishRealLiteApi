"use client";

import Link from "next/link";
import { Button } from "@/components/ui/button";
import {
  Music2,
  BarChart3,
  Users,
  Disc3,
  Video,
  ArrowRight,
  Check,
  Zap,
  Globe,
  Shield,
} from "lucide-react";

const features = [
  {
    icon: <Disc3 className="h-6 w-6" />,
    title: "Music Management",
    description:
      "Create releases, add tracks, and maintain a clean, organized catalog.",
  },
  {
    icon: <BarChart3 className="h-6 w-6" />,
    title: "Streaming Statistics",
    description:
      "Track streams by platform, country, and trends over time with real-time analytics.",
  },
  {
    icon: <Video className="h-6 w-6" />,
    title: "Video Content",
    description:
      "Upload and organize promotional videos, music videos, and behind-the-scenes content.",
  },
  {
    icon: <Users className="h-6 w-6" />,
    title: "Team Collaboration",
    description:
      "Invite managers, producers, and collaborators with role-based permissions.",
  },
];

const benefits = [
  "Unlimited music uploads",
  "Keep 100% of your earnings",
  "Real-time streaming analytics",
  "Team collaboration tools",
  "Professional artist profile",
  "Video content management",
];

export default function LandingPage() {
  return (
    <div className="min-h-screen bg-background">
      {/* Navigation */}
      <nav className="border-b border-border">
        <div className="mx-auto flex h-16 max-w-7xl items-center justify-between px-4 lg:px-8">
          <Link href="/" className="flex items-center gap-2">
            <div className="flex h-10 w-10 items-center justify-center rounded-lg bg-primary">
              <Music2 className="h-5 w-5 text-primary-foreground" />
            </div>
            <span className="text-xl font-bold text-foreground">
              PublishReal
            </span>
          </Link>
          <div className="flex items-center gap-4">
            <Link href="/login">
              <Button variant="ghost">Sign in</Button>
            </Link>
            <Link href="/register">
              <Button>Get Started</Button>
            </Link>
          </div>
        </div>
      </nav>

      {/* Hero Section */}
      <section className="relative overflow-hidden py-20 lg:py-32">
        <div className="mx-auto max-w-7xl px-4 lg:px-8">
          <div className="text-center">
            <h1 className="mx-auto max-w-4xl text-4xl font-bold tracking-tight text-foreground sm:text-5xl lg:text-6xl">
              <span className="text-balance">Share your music with the world</span>
            </h1>
            <p className="mx-auto mt-6 max-w-2xl text-lg text-muted-foreground text-pretty">
              PublishReal is the platform that empowers independent artists.
              Manage your catalog, track your stats, and collaborate with your
              team—all in one place.
            </p>
            <div className="mt-10 flex flex-col items-center justify-center gap-4 sm:flex-row">
              <Link href="/register">
                <Button size="lg" className="h-12 px-8 text-base font-semibold">
                  Get Started Free
                  <ArrowRight className="ml-2 h-4 w-4" />
                </Button>
              </Link>
              <Link href="/login">
                <Button
                  variant="outline"
                  size="lg"
                  className="h-12 px-8 text-base"
                >
                  Sign In
                </Button>
              </Link>
            </div>
          </div>
        </div>

        {/* Background gradient */}
        <div className="absolute inset-0 -z-10 overflow-hidden">
          <div className="absolute left-1/2 top-0 h-[600px] w-[600px] -translate-x-1/2 rounded-full bg-primary/10 blur-3xl" />
        </div>
      </section>

      {/* Features Section */}
      <section className="border-t border-border py-20 lg:py-28">
        <div className="mx-auto max-w-7xl px-4 lg:px-8">
          <div className="text-center">
            <h2 className="text-3xl font-bold text-foreground sm:text-4xl">
              Everything you need to grow
            </h2>
            <p className="mx-auto mt-4 max-w-2xl text-muted-foreground">
              Professional tools for independent artists, all in one intuitive
              dashboard.
            </p>
          </div>

          <div className="mt-16 grid gap-8 sm:grid-cols-2 lg:grid-cols-4">
            {features.map((feature) => (
              <div
                key={feature.title}
                className="rounded-xl border border-border bg-card p-6 transition-colors hover:border-primary/50"
              >
                <div className="flex h-12 w-12 items-center justify-center rounded-lg bg-primary/10 text-primary">
                  {feature.icon}
                </div>
                <h3 className="mt-4 text-lg font-semibold text-foreground">
                  {feature.title}
                </h3>
                <p className="mt-2 text-sm text-muted-foreground">
                  {feature.description}
                </p>
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* Benefits Section */}
      <section className="border-t border-border bg-secondary/30 py-20 lg:py-28">
        <div className="mx-auto max-w-7xl px-4 lg:px-8">
          <div className="grid gap-12 lg:grid-cols-2 lg:gap-16">
            <div>
              <h2 className="text-3xl font-bold text-foreground sm:text-4xl">
                Built for independent artists
              </h2>
              <p className="mt-4 text-lg text-muted-foreground">
                PublishReal democratizes tools that were once exclusive to major
                labels. Take control of your music career.
              </p>
              <ul className="mt-8 grid gap-3 sm:grid-cols-2">
                {benefits.map((benefit) => (
                  <li key={benefit} className="flex items-center gap-3">
                    <div className="flex h-6 w-6 items-center justify-center rounded-full bg-primary">
                      <Check className="h-4 w-4 text-primary-foreground" />
                    </div>
                    <span className="text-foreground">{benefit}</span>
                  </li>
                ))}
              </ul>
              <Link href="/register" className="mt-8 inline-block">
                <Button size="lg" className="font-semibold">
                  Start for Free
                  <ArrowRight className="ml-2 h-4 w-4" />
                </Button>
              </Link>
            </div>

            <div className="grid gap-6 sm:grid-cols-2">
              <div className="rounded-xl border border-border bg-card p-6">
                <Zap className="h-8 w-8 text-primary" />
                <h3 className="mt-4 font-semibold text-foreground">
                  Fast Distribution
                </h3>
                <p className="mt-2 text-sm text-muted-foreground">
                  Get your music on streaming platforms faster than ever.
                </p>
              </div>
              <div className="rounded-xl border border-border bg-card p-6">
                <Globe className="h-8 w-8 text-primary" />
                <h3 className="mt-4 font-semibold text-foreground">
                  Global Reach
                </h3>
                <p className="mt-2 text-sm text-muted-foreground">
                  Track your streams across 150+ countries worldwide.
                </p>
              </div>
              <div className="rounded-xl border border-border bg-card p-6">
                <Shield className="h-8 w-8 text-primary" />
                <h3 className="mt-4 font-semibold text-foreground">
                  Full Control
                </h3>
                <p className="mt-2 text-sm text-muted-foreground">
                  Own your music and keep 100% of your earnings.
                </p>
              </div>
              <div className="rounded-xl border border-border bg-card p-6">
                <Users className="h-8 w-8 text-primary" />
                <h3 className="mt-4 font-semibold text-foreground">
                  Team Ready
                </h3>
                <p className="mt-2 text-sm text-muted-foreground">
                  Collaborate securely with your entire team.
                </p>
              </div>
            </div>
          </div>
        </div>
      </section>

      {/* CTA Section */}
      <section className="border-t border-border py-20 lg:py-28">
        <div className="mx-auto max-w-7xl px-4 text-center lg:px-8">
          <h2 className="text-3xl font-bold text-foreground sm:text-4xl">
            Ready to take control of your music?
          </h2>
          <p className="mx-auto mt-4 max-w-xl text-muted-foreground">
            Join thousands of independent artists who trust PublishReal to
            manage their music careers.
          </p>
          <div className="mt-8 flex flex-col items-center justify-center gap-4 sm:flex-row">
            <Link href="/register">
              <Button size="lg" className="h-12 px-8 text-base font-semibold">
                Get Started Free
                <ArrowRight className="ml-2 h-4 w-4" />
              </Button>
            </Link>
          </div>
        </div>
      </section>

      {/* Footer */}
      <footer className="border-t border-border py-12">
        <div className="mx-auto max-w-7xl px-4 lg:px-8">
          <div className="flex flex-col items-center justify-between gap-4 sm:flex-row">
            <div className="flex items-center gap-2">
              <div className="flex h-8 w-8 items-center justify-center rounded-lg bg-primary">
                <Music2 className="h-4 w-4 text-primary-foreground" />
              </div>
              <span className="font-semibold text-foreground">PublishReal</span>
            </div>
            <p className="text-sm text-muted-foreground">
              2026 PublishReal. All rights reserved.
            </p>
          </div>
        </div>
      </footer>
    </div>
  );
}
