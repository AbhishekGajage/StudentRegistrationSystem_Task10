<%@ Page Language="C#" AutoEventWireup="true" %>

<!DOCTYPE html>
<html class="light" lang="en">
<head runat="server">
<meta charset="utf-8">
<meta content="width=device-width, initial-scale=1.0" name="viewport">
<title>New Institute - Student Registration Management System</title>
<script src="https://cdn.tailwindcss.com?plugins=forms,container-queries"></script>
<link href="https://fonts.googleapis.com/css2?family=Plus+Jakarta+Sans:wght@400;600;700;800&amp;display=swap" rel="stylesheet">
<link href="https://fonts.googleapis.com/css2?family=Material+Symbols+Outlined:wght,FILL@100..700,0..1&amp;display=swap" rel="stylesheet">
<link rel="stylesheet" href="Styles/Loader.css">
    <link rel="icon" type="image/png" href="Images/favicon.png" />
<script id="tailwind-config">
    tailwind.config = {
        darkMode: "class",
        theme: {
            extend: {
                "colors": {
                    "primary-fixed-dim": "#b4c5ff",
                    "surface-tint": "#0053db",
                    "on-primary-fixed": "#00174b",
                    "surface-container-low": "#eff4ff",
                    "on-secondary": "#ffffff",
                    "inverse-on-surface": "#eaf1ff",
                    "on-tertiary-container": "#b1fff1",
                    "surface-container-highest": "#d3e4fe",
                    "primary-fixed": "#dbe1ff",
                    "tertiary": "#006056",
                    "on-surface": "#0b1c30",
                    "primary": "#004ac6",
                    "surface-variant": "#d3e4fe",
                    "background": "#f8f9ff",
                    "secondary": "#006591",
                    "on-secondary-fixed": "#001e2f",
                    "on-error-container": "#93000a",
                    "on-background": "#0b1c30",
                    "surface-bright": "#f8f9ff",
                    "error-container": "#ffdad6",
                    "surface-container-high": "#dce9ff",
                    "surface-dim": "#cbdbf5",
                    "on-surface-variant": "#434655",
                    "on-secondary-container": "#004666",
                    "surface-container-lowest": "#ffffff",
                    "tertiary-fixed": "#71f8e4",
                    "on-secondary-fixed-variant": "#004c6e",
                    "on-tertiary-fixed-variant": "#005048",
                    "primary-container": "#2563eb",
                    "tertiary-container": "#007b6e",
                    "on-primary-container": "#eeefff",
                    "inverse-primary": "#b4c5ff",
                    "outline-variant": "#c3c6d7",
                    "on-primary": "#ffffff",
                    "inverse-surface": "#213145",
                    "secondary-fixed": "#c9e6ff",
                    "surface": "#f8f9ff",
                    "on-tertiary-fixed": "#00201c",
                    "tertiary-fixed-dim": "#4fdbc8",
                    "on-tertiary": "#ffffff",
                    "error": "#ba1a1a",
                    "on-error": "#ffffff",
                    "surface-container": "#e5eeff",
                    "on-primary-fixed-variant": "#003ea8",
                    "outline": "#737686",
                    "secondary-fixed-dim": "#89ceff",
                    "secondary-container": "#39b8fd"
                },
                "borderRadius": {
                    "DEFAULT": "0.25rem",
                    "lg": "0.5rem",
                    "xl": "0.75rem",
                    "full": "9999px"
                },
                "spacing": {
                    "margin-desktop": "40px",
                    "margin-mobile": "16px",
                    "container-max": "1280px",
                    "unit": "8px",
                    "gutter": "24px"
                },
                "fontFamily": {
                    "title-sm": ["Plus Jakarta Sans"],
                    "display-lg": ["Plus Jakarta Sans"],
                    "display-lg-mobile": ["Plus Jakarta Sans"],
                    "label-caps": ["Plus Jakarta Sans"],
                    "body-md": ["Plus Jakarta Sans"],
                    "headline-md": ["Plus Jakarta Sans"]
                },
                "fontSize": {
                    "title-sm": ["18px", { "lineHeight": "1.5", "fontWeight": "600" }],
                    "display-lg": ["48px", { "lineHeight": "1.2", "letterSpacing": "-0.02em", "fontWeight": "800" }],
                    "display-lg-mobile": ["32px", { "lineHeight": "1.2", "letterSpacing": "-0.02em", "fontWeight": "800" }],
                    "label-caps": ["12px", { "lineHeight": "1", "letterSpacing": "0.05em", "fontWeight": "700" }],
                    "body-md": ["16px", { "lineHeight": "1.6", "fontWeight": "400" }],
                    "headline-md": ["24px", { "lineHeight": "1.4", "fontWeight": "700" }]
                }
            }
        }
    }
</script>
<style>
        :root {
            --color-primary: #004ac6;
            --color-secondary: #006591;
        }
        .glass-panel {
            background: rgba(255, 255, 255, 0.6);
            backdrop-filter: blur(12px);
            -webkit-backdrop-filter: blur(12px);
            border: 1px solid rgba(255, 255, 255, 0.8);
            box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.05), 0 2px 4px -1px rgba(0, 0, 0, 0.03), inset 0 1px 0 rgba(255,255,255,0.5);
        }
        .neumorphic-lift {
            box-shadow: 4px 4px 10px rgba(175, 190, 220, 0.4), -4px -4px 10px rgba(255, 255, 255, 0.9);
            transition: transform 0.2s ease, box-shadow 0.2s ease;
        }
        .neumorphic-lift:hover {
            transform: translateY(-2px);
            box-shadow: 6px 6px 14px rgba(175, 190, 220, 0.5), -6px -6px 14px rgba(255, 255, 255, 1);
        }
        .gradient-btn {
            background: linear-gradient(135deg, var(--color-primary), var(--color-secondary));
            border: none;
            color: white;
            transition: all 0.3s ease;
        }
        .gradient-btn:hover {
            box-shadow: 0 4px 15px rgba(0, 74, 198, 0.4);
            transform: translateY(-1px);
        }
        .secondary-btn {
            background: transparent;
            border: 1.5px solid var(--color-primary);
            color: var(--color-primary);
            transition: all 0.3s ease;
        }
        .secondary-btn:hover {
            background: rgba(0, 74, 198, 0.05);
        }
        .material-symbols-outlined {
            font-variation-settings: 'FILL' 0, 'wght' 400, 'GRAD' 0, 'opsz' 24;
        }
    </style>
</head>
<body class="bg-background text-on-background font-body-md antialiased overflow-x-hidden">
<div id="pageLoader" class="page-loader">
    <div class="three-body">
        <div class="three-body__dot"></div>
        <div class="three-body__dot"></div>
        <div class="three-body__dot"></div>
    </div>
</div>
<!-- TopNavBar -->
<header class="bg-white/60 dark:bg-inverse-surface/60 backdrop-blur-xl sticky top-0 w-full z-50 rounded-b-xl border-b border-white/20 shadow-sm shadow-md">
<div class="flex justify-between items-center px-gutter py-4 max-w-container-max mx-auto">
<div class="font-headline-md text-headline-md font-extrabold text-primary dark:text-inverse-primary">
                New Institute
            </div>
<nav class="hidden md:flex gap-6 items-center">
<a class="text-on-surface-variant dark:text-outline-variant hover:text-primary dark:hover:text-primary-fixed transition-colors active:scale-95 transition-transform" href="#features">Programs</a>
<a class="text-on-surface-variant dark:text-outline-variant hover:text-primary dark:hover:text-primary-fixed transition-colors active:scale-95 transition-transform" href="#">Campus</a>
<a class="text-on-surface-variant dark:text-outline-variant hover:text-primary dark:hover:text-primary-fixed transition-colors active:scale-95 transition-transform" href="#features">Services</a>
<a class="text-on-surface-variant dark:text-outline-variant hover:text-primary dark:hover:text-primary-fixed transition-colors active:scale-95 transition-transform" href="#">About</a>
</nav>
<div class="hidden md:flex gap-4 items-center">
<button class="font-label-caps text-label-caps text-primary hover:text-primary-fixed-variant transition-colors" onclick="PageLoader.navigate('AdminLogin.aspx');">Admin Login</button>
<button class="secondary-btn px-6 py-2 rounded-lg font-label-caps text-label-caps bg-white/50 backdrop-blur-sm" onclick="PageLoader.navigate('Login.aspx');">Student Login</button>
</div>
<button class="md:hidden text-primary" onclick="document.getElementById('mobileNav').classList.toggle('hidden');">
<span class="material-symbols-outlined">menu</span>
</button>
</div>
<div id="mobileNav" class="hidden md:hidden flex flex-col gap-3 px-gutter pb-4">
<a class="text-on-surface-variant hover:text-primary transition-colors" href="#features">Programs</a>
<a class="text-on-surface-variant hover:text-primary transition-colors" href="#features">Services</a>
<button class="secondary-btn px-6 py-2 rounded-lg font-label-caps text-label-caps w-full" onclick="PageLoader.navigate('Login.aspx');">Student Login</button>
<button class="gradient-btn px-6 py-2 rounded-lg font-label-caps text-label-caps w-full" onclick="PageLoader.navigate('AdminLogin.aspx');">Admin Login</button>
</div>
</header>
<!-- Hero Section -->
<section class="relative min-h-[90vh] flex items-center justify-center pt-20 pb-32 px-gutter overflow-hidden">

<div class="max-w-container-max mx-auto w-full grid grid-cols-1 lg:grid-cols-2 gap-12 items-center">
<div class="space-y-8 z-10 relative">
<div class="inline-flex items-center gap-2 px-4 py-2 rounded-full glass-panel border border-primary/20 text-primary font-label-caps text-label-caps">
<span class="material-symbols-outlined text-[16px]">stars</span>
                    Next-Gen Registration System
                </div>
<h1 class="font-display-lg-mobile md:font-display-lg text-on-surface">
                    Student Registration Made <span class="text-transparent bg-clip-text bg-gradient-to-r from-primary to-secondary">Smart, Secure &amp; Seamless</span>
</h1>
<p class="font-body-md text-body-md text-on-surface-variant max-w-xl">
                    Manage student registrations, admissions, profile management, approvals, and academic records through one intelligent digital platform designed for high-end academic institutions.
                </p>
<div class="flex flex-col sm:flex-row gap-4 pt-4">
<button class="gradient-btn px-8 py-4 rounded-xl font-title-sm text-title-sm flex items-center justify-center gap-2" onclick="PageLoader.navigate('Register.aspx');">
                        Register Now
                        <span class="material-symbols-outlined">arrow_forward</span>
</button>
<a href="#features" class="secondary-btn px-8 py-4 rounded-xl font-title-sm text-title-sm flex items-center justify-center gap-2 bg-white/50 backdrop-blur-sm no-underline">
                        Explore Features
                        <span class="material-symbols-outlined">search</span>
</a>
</div>
<div class="flex flex-wrap gap-6 pt-8 border-t border-outline-variant/30">
<div class="flex items-center gap-2">
<span class="material-symbols-outlined text-tertiary">group</span>
<span class="font-label-caps text-label-caps text-on-surface-variant">15,000+ Students</span>
</div>
<div class="flex items-center gap-2">
<span class="material-symbols-outlined text-primary">verified_user</span>
<span class="font-label-caps text-label-caps text-on-surface-variant">Secure Registration</span>
</div>
<div class="flex items-center gap-2">
<span class="material-symbols-outlined text-secondary">dashboard</span>
<span class="font-label-caps text-label-caps text-on-surface-variant">Real-time Dashboard</span>
</div>
</div>
</div>
<div class="relative z-10 hidden lg:block">
<div class="glass-panel p-8 rounded-[24px] relative neumorphic-lift h-[500px] flex items-center justify-center">
<img class="w-full h-full object-contain rounded-xl opacity-90" alt="Abstract illustration of a digital registration dashboard" src="https://lh3.googleusercontent.com/aida-public/AB6AXuDX3X8_1ctdIkDZM4MtejLmLAjhqrGF2uidBTeg3RNCC5Uw-9Xl6Lp9279riau5u_8XJQG-fIqSu7egkhbQ9guS5kKXQ8jtLtj3vIpihhS1waTPM4gFazQsulQpZCuI2N2Yua0tsu_g00GxUo9DG9mRNLUdUevW03nwPicPSnpQpE86M6hUdWN7-pFaZ9ZcOpcxAFB118J2_qAXw0WnZkTCGwLvJYsarpXz4V07v4HQCM76pn51R6U">
<!-- Floating Elements -->
<div class="absolute -top-6 -left-6 glass-panel p-4 rounded-xl shadow-lg animate-[bounce_4s_infinite]">
<span class="material-symbols-outlined text-primary text-3xl">shield</span>
</div>
<div class="absolute top-1/4 -right-8 glass-panel p-4 rounded-xl shadow-lg animate-[bounce_5s_infinite_0.5s]">
<span class="material-symbols-outlined text-tertiary text-3xl">check_circle</span>
</div>
<div class="absolute -bottom-4 left-1/4 glass-panel p-4 rounded-xl shadow-lg animate-[bounce_6s_infinite_1s]">
<span class="material-symbols-outlined text-secondary text-3xl">bar_chart</span>
</div>
</div>
</div>
</div>
</section>
<!-- Why Choose Our Platform -->
<section id="features" class="py-24 px-gutter bg-surface-container-low/50 relative">
<div class="max-w-container-max mx-auto space-y-16">
<div class="text-center space-y-4 max-w-2xl mx-auto">
<h2 class="font-display-lg-mobile md:font-display-lg text-on-surface text-[32px] md:text-[40px]">Platform Capabilities</h2>
<p class="font-body-md text-body-md text-on-surface-variant">Experience a frictionless transition to digital management with our core modules designed for maximum efficiency.</p>
</div>
<div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-8">
<!-- Card 1 -->
<div class="glass-panel p-8 rounded-[24px] neumorphic-lift flex flex-col gap-4 group cursor-pointer hover:bg-white/80 transition-colors" onclick="PageLoader.navigate('Register.aspx');">
<div class="w-14 h-14 rounded-2xl bg-primary/10 flex items-center justify-center text-primary group-hover:scale-110 transition-transform">
<span class="material-symbols-outlined text-[32px]">how_to_reg</span>
</div>
<h3 class="font-title-sm text-title-sm text-on-surface">Student Registration</h3>
<p class="font-body-md text-body-md text-on-surface-variant text-sm">Streamlined multi-step forms with dynamic field validation and secure document uploads.</p>
</div>
<!-- Card 2 -->
<div class="glass-panel p-8 rounded-[24px] neumorphic-lift flex flex-col gap-4 group cursor-pointer hover:bg-white/80 transition-colors" onclick="PageLoader.navigate('Login.aspx');">
<div class="w-14 h-14 rounded-2xl bg-secondary/10 flex items-center justify-center text-secondary group-hover:scale-110 transition-transform">
<span class="material-symbols-outlined text-[32px]">lock_person</span>
</div>
<h3 class="font-title-sm text-title-sm text-on-surface">Secure Login</h3>
<p class="font-body-md text-body-md text-on-surface-variant text-sm">CAPTCHA-protected sign-in ensures only verified students can access their records.</p>
</div>
<!-- Card 3 -->
<div class="glass-panel p-8 rounded-[24px] neumorphic-lift flex flex-col gap-4 group cursor-pointer hover:bg-white/80 transition-colors" onclick="PageLoader.navigate('Login.aspx');">
<div class="w-14 h-14 rounded-2xl bg-tertiary/10 flex items-center justify-center text-tertiary group-hover:scale-110 transition-transform">
<span class="material-symbols-outlined text-[32px]">space_dashboard</span>
</div>
<h3 class="font-title-sm text-title-sm text-on-surface">Student Dashboard</h3>
<p class="font-body-md text-body-md text-on-surface-variant text-sm">Personalized hubs for students to track status, view profiles, and manage enrollment data.</p>
</div>
<!-- Card 4 -->
<div class="glass-panel p-8 rounded-[24px] neumorphic-lift flex flex-col gap-4 group cursor-pointer hover:bg-white/80 transition-colors" onclick="PageLoader.navigate('AdminLogin.aspx');">
<div class="w-14 h-14 rounded-2xl bg-primary/10 flex items-center justify-center text-primary group-hover:scale-110 transition-transform">
<span class="material-symbols-outlined text-[32px]">fact_check</span>
</div>
<h3 class="font-title-sm text-title-sm text-on-surface">Application Review</h3>
<p class="font-body-md text-body-md text-on-surface-variant text-sm">Admins approve, reject, or reset applications, with a full audit trail for every decision.</p>
</div>
<!-- Card 5 -->
<div class="glass-panel p-8 rounded-[24px] neumorphic-lift flex flex-col gap-4 group cursor-pointer hover:bg-white/80 transition-colors" onclick="PageLoader.navigate('AdminLogin.aspx');">
<div class="w-14 h-14 rounded-2xl bg-secondary/10 flex items-center justify-center text-secondary group-hover:scale-110 transition-transform">
<span class="material-symbols-outlined text-[32px]">manage_search</span>
</div>
<h3 class="font-title-sm text-title-sm text-on-surface">Advanced Search</h3>
<p class="font-body-md text-body-md text-on-surface-variant text-sm">Powerful filtering by name, email, approval status, and account status across all records.</p>
</div>
<!-- Card 6 -->
<div class="glass-panel p-8 rounded-[24px] neumorphic-lift flex flex-col gap-4 group cursor-pointer hover:bg-white/80 transition-colors" onclick="PageLoader.navigate('AdminLogin.aspx');">
<div class="w-14 h-14 rounded-2xl bg-tertiary/10 flex items-center justify-center text-tertiary group-hover:scale-110 transition-transform">
<span class="material-symbols-outlined text-[32px]">admin_panel_settings</span>
</div>
<h3 class="font-title-sm text-title-sm text-on-surface">Admin Management</h3>
<p class="font-body-md text-body-md text-on-surface-variant text-sm">Comprehensive control center for application reviews, approvals, and role-based access.</p>
</div>
</div>
</div>
</section>
<!-- Footer -->
<footer class="bg-surface-container-low dark:bg-inverse-surface w-full rounded-t-xl border-t border-outline-variant/30 mt-24">
<div class="grid grid-cols-2 md:grid-cols-4 gap-8 px-gutter py-margin-desktop max-w-container-max mx-auto">
<div class="col-span-2 md:col-span-1 space-y-4">
<div class="font-headline-md text-headline-md font-bold text-on-surface dark:text-inverse-on-surface mb-6">New Institute</div>
<p class="font-body-md text-body-md text-on-surface-variant text-sm">Empowering the future of education with intelligent administrative solutions.</p>
<div class="font-body-md text-body-md font-label-caps text-label-caps text-on-surface-variant pt-4">&copy; 2026 New Institute. All rights reserved.</div>
</div>
<div class="space-y-4">
<h4 class="font-label-caps text-label-caps text-primary dark:text-inverse-primary mb-4">Quick Links</h4>
<ul class="space-y-3 font-body-md text-body-md text-on-surface-variant dark:text-outline-variant">
<li><a class="hover:text-secondary dark:hover:text-secondary-fixed transition-colors" href="#">Academic Calendar</a></li>
<li><a class="hover:text-secondary dark:hover:text-secondary-fixed transition-colors" href="#">Financial Aid</a></li>
<li><a class="hover:text-secondary dark:hover:text-secondary-fixed transition-colors" href="#">Student Support</a></li>
<li><a class="hover:text-secondary dark:hover:text-secondary-fixed transition-colors" href="#features">Services</a></li>
</ul>
</div>
<div class="space-y-4">
<h4 class="font-label-caps text-label-caps text-primary dark:text-inverse-primary mb-4">Campus</h4>
<ul class="space-y-3 font-body-md text-body-md text-on-surface-variant dark:text-outline-variant">
<li><a class="hover:text-secondary dark:hover:text-secondary-fixed transition-colors" href="#">Enrollment</a></li>
<li><a class="hover:text-secondary dark:hover:text-secondary-fixed transition-colors" href="#">Campus Safety</a></li>
<li><a class="hover:text-secondary dark:hover:text-secondary-fixed transition-colors" href="#">Library</a></li>
</ul>
</div>
<div class="space-y-4">
<h4 class="font-label-caps text-label-caps text-primary dark:text-inverse-primary mb-4">Contact</h4>
<ul class="space-y-3 font-body-md text-body-md text-on-surface-variant dark:text-outline-variant">
<li><a class="hover:text-secondary dark:hover:text-secondary-fixed transition-colors" href="#">Admissions Office</a></li>
<li><a class="hover:text-secondary dark:hover:text-secondary-fixed transition-colors" href="#">Registrar</a></li>
<li><a class="hover:text-secondary dark:hover:text-secondary-fixed transition-colors" href="#">General Inquiry</a></li>
</ul>
</div>
</div>
</footer>

<script src="Scripts/loader.js"></script>
</body>
</html>
