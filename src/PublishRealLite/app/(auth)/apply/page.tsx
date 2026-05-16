"use client";

import { useState } from "react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { apiClient } from "@/lib/api/client";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { RadioGroup, RadioGroupItem } from "@/components/ui/radio-group";
import { TurnstileWidget } from "@/components/ui/turnstile-widget";
import { LoadingSpinner } from "@/components/loading-states";
import { Music2, Check, ArrowLeft, Instagram, Globe } from "lucide-react";

// 1. Validation Schema
const applySchema = z.object({
  nombre: z.string().min(2, "Artist name is required"),
  email: z.string().email("Please enter a valid email address"),
  pais: z.string().min(2, "Country is required"),
  instagram: z.string().url("Please enter a valid URL (https://...)"),
  definicion: z.enum(["Compositor", "Artista", "Ambos"], {
    required_error: "Please select how you define yourself",
  }),
  cancion_compositor: z.string().url("Valid URL required").optional().or(z.literal("")),
  cancion_artista: z.string().url("Valid URL required").optional().or(z.literal("")),
  pro: z.string().min(1, "Required"),
  derechos: z.string().min(1, "Required"),
  acuerdo: z.string().min(1, "Required"),
}).refine((data) => {
  if ((data.definicion === "Compositor" || data.definicion === "Ambos") && !data.cancion_compositor) return false;
  return true;
}, { message: "Composer song link is required", path: ["cancion_compositor"] })
.refine((data) => {
  if ((data.definicion === "Artista" || data.definicion === "Ambos") && !data.cancion_artista) return false;
  return true;
}, { message: "Artist song link is required", path: ["cancion_artista"] });

type ApplyFormData = z.infer<typeof applySchema>;

export default function ApplyPage() {
  const router = useRouter();
  const [isLoading, setIsLoading] = useState(false);
  const [submitted, setSubmitted] = useState(false);
  const [lang, setLang] = useState<"es" | "en">("es");
  const [turnstileToken, setTurnstileToken] = useState<string | null>(null);

  const {
    register,
    handleSubmit,
    watch,
    setValue,
    formState: { errors },
  } = useForm<ApplyFormData>({
    resolver: zodResolver(applySchema),
  });

  const selectedRole = watch("definicion");

  const onSubmit = async (data: ApplyFormData) => {
    setIsLoading(true);
    try {
      await apiClient.submitApplication({
        artistName: data.nombre,
        email: data.email,
        country: data.pais,
        instagramUrl: data.instagram,
        role: data.definicion,
        songAsComposerUrl: data.cancion_compositor || undefined,
        songAsArtistUrl: data.cancion_artista || undefined,
        affiliatedWithPro: data.pro === "Sí",
        ownershipType: data.derechos,
        interestedInSigning: data.acuerdo === "Sí",
        turnstileToken: turnstileToken!,
      });
      setSubmitted(true);
    } catch (err) {
      console.error("Submission error:", err);
      alert("Error sending application. Please try again.");
    } finally {
      setIsLoading(false);
    }
  };

  const content = {
    es: {
      title: "Aplicación – La Realeza Publishing",
      desc: "Somos una publisher enfocada en el desarrollo y la monetización de composiciones. Este proceso toma menos de 1 minuto.",
      btn: "Enviar Aplicación",
      back: "Volver",
      success: "¡Aplicación Enviada!",
      successDesc: "Revisaremos tu perfil y te contactaremos pronto.",
    },
    en: {
      title: "Application – La Realeza Publishing",
      desc: "We are a publisher focused on the development and monetization of compositions. This takes less than 1 minute.",
      btn: "Submit Application",
      back: "Back",
      success: "Application Sent!",
      successDesc: "We will review your profile and contact you soon.",
    }
  };

  if (submitted) {
    return (
      <div className="flex min-h-screen items-center justify-center bg-background p-4 animate-in fade-in duration-500">
        <div className="text-center space-y-6">
          <div className="mx-auto flex h-20 w-20 items-center justify-center rounded-full bg-primary/20">
            <Check className="h-10 w-10 text-primary" />
          </div>
          <div className="space-y-2">
            <h1 className="text-3xl font-bold">{content[lang].success}</h1>
            <p className="text-muted-foreground">{content[lang].successDesc}</p>
          </div>
          <Button asChild size="lg" className="rounded-full">
            <Link href="/">Ir al Inicio</Link>
          </Button>
        </div>
      </div>
    );
  }

  return (
    <div className="flex min-h-screen bg-background">
      {/* Language Switcher */}
      <div className="fixed top-6 right-6 z-50 flex gap-1 bg-card border rounded-full p-1 shadow-sm">
        <Button 
          variant={lang === "es" ? "default" : "ghost"} 
          size="sm" 
          className="rounded-full px-4"
          onClick={() => setLang("es")}
        >
          ES
        </Button>
        <Button 
          variant={lang === "en" ? "default" : "ghost"} 
          size="sm" 
          className="rounded-full px-4"
          onClick={() => setLang("en")}
        >
          EN
        </Button>
      </div>

      <div className="flex flex-1 flex-col items-center justify-start overflow-y-auto px-6 py-12 lg:py-20">
        <div className="w-full max-w-xl">
          
          {/* 3. Navigation Bar with Back Button */}
          <div className="mb-10 flex items-center justify-between">
            <Button 
              variant="ghost" 
              onClick={() => router.back()} 
              className="group flex items-center gap-2 text-muted-foreground hover:text-foreground transition-colors"
            >
              <ArrowLeft className="h-4 w-4 group-hover:-translate-x-1 transition-transform" />
              {content[lang].back}
            </Button>

            <Link href="/" className="inline-flex items-center gap-2">
              <div className="flex h-10 w-10 items-center justify-center rounded-xl bg-primary shadow-lg shadow-primary/20">
                <Music2 className="h-5 w-5 text-primary-foreground" />
              </div>
              <span className="text-xl font-bold tracking-tight">La Realeza</span>
            </Link>
          </div>

          <div className="mb-8 text-center lg:text-left">
            <h1 className="text-3xl font-extrabold tracking-tight mb-3">
              {content[lang].title}
            </h1>
            <p className="text-muted-foreground leading-relaxed">
              {content[lang].desc}
            </p>
          </div>

          <div className="rounded-2xl border border-border bg-card p-8 shadow-sm">
            <form onSubmit={handleSubmit(onSubmit)} className="space-y-8">
              
              {/* Personal Info Grid */}
              <div className="grid grid-cols-1 gap-6 sm:grid-cols-2">
                <div className="space-y-2">
                  <Label>{lang === "es" ? "Nombre artístico" : "Artist Name"} *</Label>
                  <Input {...register("nombre")} placeholder="e.g. Bad Bunny" className="h-11" />
                  {errors.nombre && <p className="text-xs text-destructive">{errors.nombre.message}</p>}
                </div>
                <div className="space-y-2">
                  <Label>Email *</Label>
                  <Input {...register("email")} type="email" placeholder="artista@email.com" className="h-11" />
                  {errors.email && <p className="text-xs text-destructive">{errors.email.message}</p>}
                </div>
              </div>

              <div className="grid grid-cols-1 gap-6 sm:grid-cols-2">
                <div className="space-y-2">
                  <Label>{lang === "es" ? "País" : "Country"} *</Label>
                  <Input {...register("pais")} placeholder="República Dominicana" className="h-11" />
                </div>
                <div className="space-y-2">
                  <Label>Instagram URL *</Label>
                  <Input {...register("instagram")} placeholder="https://instagram.com/..." className="h-11" />
                </div>
              </div>

              {/* Define Role Section */}
              <div className="space-y-4">
                <Label className="text-base">{lang === "es" ? "¿Cómo te defines?" : "How do you define yourself?"}</Label>
                <RadioGroup 
                    onValueChange={(v) => setValue("definicion", v as any)} 
                    className="grid grid-cols-1 sm:grid-cols-3 gap-4"
                >
                  {["Compositor", "Artista", "Ambos"].map((role) => (
                    <div key={role}>
                      <RadioGroupItem value={role} id={role} className="peer sr-only" />
                      <Label
                        htmlFor={role}
                        className="flex flex-col items-center justify-between rounded-lg border-2 border-muted bg-popover p-4 hover:bg-accent peer-data-[state=checked]:border-primary [&:has([data-state=checked])]:border-primary cursor-pointer transition-all"
                      >
                        <span className="font-semibold text-sm">{role}</span>
                      </Label>
                    </div>
                  ))}
                </RadioGroup>
                {errors.definicion && <p className="text-xs text-destructive">{errors.definicion.message}</p>}
              </div>

              {/* Conditional Music Fields */}
              {(selectedRole === "Compositor" || selectedRole === "Ambos") && (
                <div className="space-y-2 p-4 rounded-xl bg-primary/5 border border-primary/10 animate-in slide-in-from-top-2">
                  <Label className="text-primary font-semibold text-sm">
                    {lang === "es" ? "Canción como compositor" : "Song as composer"}
                  </Label>
                  <Input {...register("cancion_compositor")} placeholder="https://..." className="bg-background" />
                  {errors.cancion_compositor && <p className="text-xs text-destructive">{errors.cancion_compositor.message}</p>}
                </div>
              )}

              {(selectedRole === "Artista" || selectedRole === "Ambos") && (
                <div className="space-y-2 p-4 rounded-xl bg-primary/5 border border-primary/10 animate-in slide-in-from-top-2">
                  <Label className="text-primary font-semibold text-sm">
                    {lang === "es" ? "Una de tus canciones" : "One of your songs"}
                  </Label>
                  <Input {...register("cancion_artista")} placeholder="https://..." className="bg-background" />
                  {errors.cancion_artista && <p className="text-xs text-destructive">{errors.cancion_artista.message}</p>}
                </div>
              )}

              {/* Additional Questions */}
              <div className="space-y-6 pt-4 border-t">
                <div className="space-y-3">
                    <Label className="text-sm font-medium">{lang === "es" ? "¿Afiliado a PRO?" : "Affiliated with PRO?"}</Label>
                    <RadioGroup onValueChange={(v) => setValue("pro", v)} className="flex gap-6">
                        <div className="flex items-center space-x-2"><RadioGroupItem value="Sí" id="pro-y" /><Label htmlFor="pro-y">Sí</Label></div>
                        <div className="flex items-center space-x-2"><RadioGroupItem value="No" id="pro-n" /><Label htmlFor="pro-n">No</Label></div>
                    </RadioGroup>
                </div>

                <div className="space-y-3">
                    <Label className="text-sm font-medium">{lang === "es" ? "¿Dueño de derechos?" : "Owner of rights?"}</Label>
                    <RadioGroup onValueChange={(v) => setValue("derechos", v)} className="grid grid-cols-1 gap-2">
                        <div className="flex items-center space-x-2"><RadioGroupItem value="Total" id="r-t" /><Label htmlFor="r-t">{lang === "es" ? "Sí, total" : "Yes, total"}</Label></div>
                        <div className="flex items-center space-x-2"><RadioGroupItem value="Parcial" id="r-p" /><Label htmlFor="r-p">{lang === "es" ? "Parcial" : "Partial"}</Label></div>
                    </RadioGroup>
                </div>

                <div className="space-y-3">
                    <Label className="text-sm font-medium">{lang === "es" ? "¿Interés en firmar?" : "Interest in signing?"}</Label>
                    <RadioGroup onValueChange={(v) => setValue("acuerdo", v)} className="flex gap-6">
                        <div className="flex items-center space-x-2"><RadioGroupItem value="Sí" id="s-y" /><Label htmlFor="s-y">Sí</Label></div>
                        <div className="flex items-center space-x-2"><RadioGroupItem value="No" id="s-n" /><Label htmlFor="s-n">No</Label></div>
                    </RadioGroup>
                </div>
              </div>

              <TurnstileWidget
                onSuccess={setTurnstileToken}
                onExpire={() => setTurnstileToken(null)}
              />

              <Button type="submit" className="w-full h-14 text-lg font-bold shadow-lg shadow-primary/20" disabled={isLoading || !turnstileToken}>
                {isLoading ? <LoadingSpinner size="sm" className="mr-2" /> : null}
                {isLoading ? (lang === "es" ? "Enviando..." : "Sending...") : content[lang].btn}
              </Button>
            </form>
          </div>
        </div>
      </div>

      {/* Decorative Right Panel */}
      <div className="hidden lg:flex flex-1 items-center justify-center bg-secondary/30 border-l relative">
        <div className="max-w-md px-12 space-y-6">
          <div className="inline-flex p-4 rounded-2xl bg-primary/10">
            <Music2 className="h-10 w-10 text-primary" />
          </div>
          <h2 className="text-4xl font-bold tracking-tight leading-tight">
            {lang === "es" ? "Eleva tu música al siguiente nivel" : "Take your music to the next level"}
          </h2>
          <p className="text-lg text-muted-foreground leading-relaxed">
            {lang === "es" 
              ? "Únete a una red diseñada para potenciar cada composición."
              : "Join a network designed to empower every composition."}
          </p>
        </div>
      </div>
    </div>
  );
}