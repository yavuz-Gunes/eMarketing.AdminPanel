import { mkdir, copyFile } from "node:fs/promises";
import { dirname, join } from "node:path";
import { fileURLToPath } from "node:url";

const root = dirname(dirname(fileURLToPath(import.meta.url)));
const source = join(root, "node_modules", "cropperjs", "dist");
const target = join(root, "wwwroot", "lib", "cropperjs");

await mkdir(target, { recursive: true });
await copyFile(join(source, "cropper.min.js"), join(target, "cropper.min.js"));
await copyFile(join(source, "cropper.min.css"), join(target, "cropper.min.css"));
