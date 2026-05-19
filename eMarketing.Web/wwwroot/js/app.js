const authStorageKey = "emarketing.auth";
const authCookieName = "emarketing.auth";
const activeStoreStorageKey = "emarketing.activeStoreId";
const activeStoreCookieName = "emarketing.activeStoreId";

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

function writeActiveStoreCookie(storeId) {
  const expires = new Date();
  expires.setFullYear(expires.getFullYear() + 1);
  document.cookie = `${activeStoreCookieName}=${encodeURIComponent(String(storeId))}; expires=${expires.toUTCString()}; path=/; samesite=lax`;
}

function clearActiveStoreCookie() {
  document.cookie = `${activeStoreCookieName}=; expires=Thu, 01 Jan 1970 00:00:00 GMT; path=/; samesite=lax`;
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
let activeProductCropper;

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

function destroyProductCropper() {
  if (activeProductCropper) {
    activeProductCropper.destroy();
    activeProductCropper = undefined;
  }
}

async function loadProductCropper(inputId, imageId, previewSelector, maxBytes) {
  if (!window.Cropper) {
    throw new Error("Gorsel kirpma araci yuklenemedi. Sayfayi yenileyip tekrar deneyin.");
  }

  const file = readCampaignFile(inputId, maxBytes);
  const dataUrl = await readFileAsDataUrl(file);
  const image = document.getElementById(imageId);

  if (!image) {
    throw new Error("Gorsel onizleme alani bulunamadi.");
  }

  destroyProductCropper();
  image.src = dataUrl;

  await new Promise((resolve, reject) => {
    image.onload = resolve;
    image.onerror = () => reject(new Error("Gorsel okunamadi."));
  });

  activeProductCropper = new window.Cropper(image, {
    aspectRatio: 16 / 11,
    viewMode: 1,
    dragMode: "move",
    autoCropArea: 0.9,
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

function cropProductImage(width, height, quality) {
  if (!activeProductCropper) {
    throw new Error("Kaydetmeden once gorsel secip kirpma alanini belirleyin.");
  }

  const canvas = activeProductCropper.getCroppedCanvas({
    width,
    height,
    imageSmoothingEnabled: true,
    imageSmoothingQuality: "high",
    fillColor: "#ffffff"
  });

  if (!canvas) {
    throw new Error("Kirpilmis gorsel hazirlanamadi.");
  }

  return canvas.toDataURL("image/jpeg", quality ?? 0.9);
}

window.eMarketing = {
  focus: (selector) => {
    const element = document.querySelector(selector);
    if (element) {
      element.focus();
    }
  },
  scrollTop: () => window.scrollTo({ top: 0, behavior: "smooth" }),
  print: () => window.print(),
  printElement: (elementId, title) => {
    const element = document.getElementById(elementId);
    if (!element) {
      throw new Error("Yazdirilacak rapor bulunamadi.");
    }

    const printWindow = window.open("", "_blank", "width=980,height=900");
    if (!printWindow) {
      throw new Error("Yazdirma penceresi acilamadi. Tarayici popup engelini kontrol edin.");
    }

    printWindow.document.write(`<!doctype html>
<html lang="tr">
<head>
  <meta charset="utf-8">
  <title>${title || "Rapor"}</title>
  <style>
    * { box-sizing: border-box; }
    body { margin: 0; background: #f1f5f9; color: #0f172a; font-family: Inter, Arial, sans-serif; }
    article { width: 210mm; min-height: 297mm; margin: 0 auto; background: #fff; padding: 24px; }
    .shadow-xl { box-shadow: none !important; }
    @page { size: A4; margin: 12mm; }
    @media print {
      body { background: #fff; }
      article { width: auto; min-height: auto; margin: 0; padding: 0; }
    }
  </style>
</head>
<body>${element.outerHTML}</body>
</html>`);
    printWindow.document.close();
    printWindow.focus();
    setTimeout(() => {
      printWindow.print();
      printWindow.close();
    }, 250);
  },
  files: {
    downloadBase64: (fileName, contentType, base64) => {
      const link = document.createElement("a");
      link.href = `data:${contentType};base64,${base64}`;
      link.download = fileName || "rapor.pdf";
      document.body.appendChild(link);
      link.click();
      link.remove();
    },
    downloadElementPdf: async (elementId, fileName) => {
      const element = document.getElementById(elementId);
      if (!element) {
        throw new Error("PDF'e aktarilacak rapor bulunamadi.");
      }

      if (!window.html2pdf) {
        throw new Error("PDF aktarim araci yuklenemedi. Sayfayi yenileyip tekrar deneyin.");
      }

      await window.html2pdf()
        .set({
          filename: fileName || "rapor.pdf",
          margin: [0, 0, 0, 0],
          image: { type: "jpeg", quality: 0.98 },
          html2canvas: {
            scale: 2,
            useCORS: true,
            backgroundColor: "#ffffff"
          },
          jsPDF: {
            unit: "mm",
            format: "a4",
            orientation: "portrait"
          },
          pagebreak: { mode: ["avoid-all", "css", "legacy"] }
        })
        .from(element)
        .save();
    }
  },
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
  store: {
    save: (storeId) => {
      localStorage.setItem(activeStoreStorageKey, String(storeId));
      writeActiveStoreCookie(storeId);
    },
    clear: () => {
      localStorage.removeItem(activeStoreStorageKey);
      clearActiveStoreCookie();
    }
  },
  campaignCrop: {
    load: loadCampaignCropper,
    cropToDataUrl: cropCampaignImage,
    destroy: destroyCampaignCropper
  },
  productCrop: {
    load: loadProductCropper,
    cropToDataUrl: cropProductImage,
    destroy: destroyProductCropper
  }
};
