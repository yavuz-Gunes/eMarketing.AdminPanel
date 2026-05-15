const authStorageKey = "emarketing.auth";
const authCookieName = "emarketing.auth";

function writeAuthCookie(value, expiresAt) {
  const expires = new Date(expiresAt);
  if (Number.isNaN(expires.getTime())) {
    return;
  }

  document.cookie = `${authCookieName}=${encodeURIComponent(value)}; expires=${expires.toUTCString()}; path=/; samesite=lax`;
}

function clearAuthCookie() {
  document.cookie = `${authCookieName}=; expires=Thu, 01 Jan 1970 00:00:00 GMT; path=/; samesite=lax`;
}

function readCampaignFile(inputId, maxBytes) {
  const input = document.getElementById(inputId);
  const file = input?.files?.[0];

  if (!file) {
    throw new Error("Görsel dosyası seçilmedi.");
  }

  if (file.size > maxBytes) {
    throw new Error("Görsel en fazla 5 MB olabilir.");
  }

  return file;
}

function loadImageFromDataUrl(dataUrl) {
  return new Promise((resolve, reject) => {
    const image = new Image();
    image.onload = () => resolve(image);
    image.onerror = () => reject(new Error("Görsel okunamadı."));
    image.src = dataUrl;
  });
}

function readFileAsDataUrl(file) {
  return new Promise((resolve, reject) => {
    const reader = new FileReader();
    reader.onload = () => resolve(reader.result);
    reader.onerror = () => reject(new Error("Görsel okunamadı."));
    reader.readAsDataURL(file);
  });
}

function getCropValue(crop, camelName, pascalName, fallback) {
  const value = crop?.[camelName] ?? crop?.[pascalName] ?? fallback;
  return Number.isFinite(Number(value)) ? Number(value) : fallback;
}

async function cropCampaignImage(inputId, crop, maxBytes) {
  const file = readCampaignFile(inputId, maxBytes);
  const dataUrl = await readFileAsDataUrl(file);
  const image = await loadImageFromDataUrl(dataUrl);
  const canvas = document.createElement("canvas");
  const width = 1600;
  const height = 220;
  const zoom = Math.max(1, getCropValue(crop, "zoom", "Zoom", 1));
  const offsetX = Math.max(-100, Math.min(100, getCropValue(crop, "offsetX", "OffsetX", 0)));
  const offsetY = Math.max(-100, Math.min(100, getCropValue(crop, "offsetY", "OffsetY", 0)));

  canvas.width = width;
  canvas.height = height;

  const context = canvas.getContext("2d");
  context.imageSmoothingEnabled = true;
  context.imageSmoothingQuality = "high";
  context.fillStyle = "#f8fafc";
  context.fillRect(0, 0, width, height);

  const scale = Math.max(width / image.naturalWidth, height / image.naturalHeight) * zoom;
  const drawWidth = image.naturalWidth * scale;
  const drawHeight = image.naturalHeight * scale;
  const maxPanX = Math.max(0, (drawWidth - width) / 2);
  const maxPanY = Math.max(0, (drawHeight - height) / 2);
  const drawX = (width - drawWidth) / 2 + (offsetX / 100) * maxPanX;
  const drawY = (height - drawHeight) / 2 + (offsetY / 100) * maxPanY;

  context.drawImage(image, drawX, drawY, drawWidth, drawHeight);
  return canvas.toDataURL("image/jpeg", 0.92);
}

window.eMarketing = {
  focus: (selector) => {
    const element = document.querySelector(selector);
    if (element) {
      element.focus();
    }
  },
  scrollTop: () => window.scrollTo({ top: 0, behavior: "smooth" }),
  auth: {
    save: (session) => {
      const value = JSON.stringify(session);
      localStorage.setItem(authStorageKey, value);
      writeAuthCookie(value, session.expiresAt);
    },
    clear: () => {
      localStorage.removeItem(authStorageKey);
      clearAuthCookie();
    }
  },
  campaignCrop: {
    load: async (inputId, maxBytes) => {
      const file = readCampaignFile(inputId, maxBytes);
      const dataUrl = await readFileAsDataUrl(file);
      const image = await loadImageFromDataUrl(dataUrl);

      return {
        dataUrl,
        fileName: file.name,
        width: image.naturalWidth,
        height: image.naturalHeight
      };
    },
    cropToDataUrl: cropCampaignImage
  }
};
