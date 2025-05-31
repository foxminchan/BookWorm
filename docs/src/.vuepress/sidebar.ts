import { sidebar } from "vuepress-theme-hope";

export default sidebar({
  "/": [
    "",
    {
      text: "Architecture",
      icon: "diagram-project",
      prefix: "architecture/",
      children: "structure",
    },
  ],
});
