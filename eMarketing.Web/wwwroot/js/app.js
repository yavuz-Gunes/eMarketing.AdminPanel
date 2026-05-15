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
    throw new Error("Gorsel dosyasi secilmedi.");
  }

  if (file.size > maxBytes) {
    throw new Error("Gorsel en fazla 5 MB olabilir.");
  }

  return file;
}

function readFileAsDataUrl(file) {
  return new Promise((resolve, reject) => {
    const reader = new FileReader();
    reader.onload = () => resolve(reader.result);
    reader.onerror = () => reject(new Error("Gorsel okunamadi."));
    reader.readAsDataURL(file);
  });
}

let activeCampaignCropper;

function destroyCampaignCropper() {
  if (activeCampaignCropper) {
    activeCampaignCropper.destroy();
    activeCampaignCropper = undefined;
  }
}

async function loadCampaignCropper(inputId, imageId, previewSelector, maxBytes) {
  if (!window.Cropper) {
    throw new Error("Gorsel kirpma araci yuklenemedi. Sayfayi yenileyip tekrar deneyin.");
  }

  const file = readCampaignFile(inputId, maxBytes);
  const dataUrl = await readFileAsDataUrl(file);
  const image = document.getElementById(imageId);

  if (!image) {
    throw new Error("Gorsel onizleme alani bulunamadi.");
  }

  destroyCampaignCropper();
  image.src = dataUrl;

  await new Promise((resolve, reject) => {
    image.onload = resolve;
    image.onerror = () => reject(new Error("Gorsel okunamadi."));
  });

  activeCampaignCropper = new window.Cropper(image, {
    aspectRatio: 1600 / 220,
    viewMode: 1,
    dragMode: "move",
    autoCropArea: 0.92,
    background: false,
    responsive: true,
    restore: false,
    checkOrientation: true,
    guides: true,
    center: true,
    highlight: true,
    cropBoxMovable: true,
    cropBoxResizable: true,
    toggleDragModeOnDblclick: false,
    preview: previewSelector
  });

  return {
    dataUrl,
    fileName: file.name
  };
}

function cropCampaignImage(width, height, quality) {
  if (!activeCampaignCropper) {
    throw new Error("Kaydetmeden once gorsel secip kirpma alanini belirleyin.");
  }

  const canvas = activeCampaignCropper.getCroppedCanvas({
    width,
    height,
    imageSmoothingEnabled: true,
    imageSmoothingQuality: "high",
    fillColor: "#ffffff"
  });

  if (!canvas) {
    throw new Error("Kirpilmis gorsel hazirlanamadi.");
  }

  return canvas.toDataURL("image/jpeg", quality ?? 0.92);
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
    load: loadCampaignCropper,
    cropToDataUrl: cropCampaignImage,
    destroy: destroyCampaignCropper
  }
};
