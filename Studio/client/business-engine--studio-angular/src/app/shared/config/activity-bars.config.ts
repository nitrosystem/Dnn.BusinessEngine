import { ActivityBar } from "../../ui-models/activity-bar";

export const activityBars: ActivityBar[] = [
    {
        name: "application-menu",
        title: "Application Menu",
        icon: "menu",
    },
    {
        name: "explorer",
        title: "Explorer",
        icon: "files",
    },
    {
        name: "builder",
        title: "Builder",
        icon: "symbol-color",
    },
    {
        name: "extensions",
        title: "Extensions",
        icon: "extensions",
        callback: "onGotoExtensions"
    }
];
