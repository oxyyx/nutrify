import { IconCategories, IconDashboard, IconFood, IconIntake } from "./nav";

// Nav destinations shared by the desktop Sidebar and the mobile bottom tab bar.
export const navItems = [
  { to: "/dashboard", label: "Dashboard", shortLabel: "Dashboard", Icon: IconDashboard },
  { to: "/intake", label: "Intake Log", shortLabel: "Intake", Icon: IconIntake },
  { to: "/food-items", label: "Food Items", shortLabel: "Food", Icon: IconFood },
  { to: "/categories", label: "Categories", shortLabel: "Categories", Icon: IconCategories },
] as const;
