import { useEffect, useRef, useState } from "react";
import { BrowserMultiFormatReader, type IScannerControls } from "@zxing/browser";
import { BarcodeFormat, DecodeHintType } from "@zxing/library";
import { ErrorBanner } from "@/shared/components/ErrorBanner";

interface BarcodeScannerProps {
  /** Called for every decoded barcode; fires repeatedly while a code is in view, so debounce in the parent. */
  onDetected: (barcode: string) => void;
}

const PRODUCT_FORMATS = [
  BarcodeFormat.EAN_13,
  BarcodeFormat.EAN_8,
  BarcodeFormat.UPC_A,
  BarcodeFormat.UPC_E,
  BarcodeFormat.CODE_128,
];

function cameraErrorMessage(err: unknown): string {
  if (err instanceof DOMException) {
    if (err.name === "NotAllowedError") {
      return "Camera access was denied. Allow camera access in your browser settings, or enter the barcode manually below.";
    }
    if (err.name === "NotFoundError" || err.name === "OverconstrainedError") {
      return "No camera was found on this device. You can still enter the barcode manually below.";
    }
  }
  return "Couldn't start the camera. You can still enter the barcode manually below.";
}

export function BarcodeScanner({ onDetected }: BarcodeScannerProps) {
  const videoRef = useRef<HTMLVideoElement>(null);
  const onDetectedRef = useRef(onDetected);

  useEffect(() => {
    onDetectedRef.current = onDetected;
  });

  // getUserMedia is only exposed in secure contexts (HTTPS or localhost).
  const isSupported = !!navigator.mediaDevices?.getUserMedia;

  const [cameraError, setCameraError] = useState<string | null>(
    isSupported
      ? null
      : "Camera scanning needs a secure connection (HTTPS). You can still enter the barcode manually below.",
  );
  const [isStarting, setIsStarting] = useState(isSupported);

  useEffect(() => {
    const video = videoRef.current;
    if (!video || !isSupported) return;

    const hints = new Map<DecodeHintType, unknown>([
      [DecodeHintType.POSSIBLE_FORMATS, PRODUCT_FORMATS],
    ]);
    const reader = new BrowserMultiFormatReader(hints);

    let controls: IScannerControls | undefined;
    let cancelled = false;

    reader
      .decodeFromConstraints(
        { video: { facingMode: "environment" } },
        video,
        (result) => {
          if (result) onDetectedRef.current(result.getText());
        },
      )
      .then((c) => {
        if (cancelled) {
          c.stop();
        } else {
          controls = c;
          setIsStarting(false);
        }
      })
      .catch((err: unknown) => {
        if (!cancelled) {
          setCameraError(cameraErrorMessage(err));
          setIsStarting(false);
        }
      });

    return () => {
      cancelled = true;
      controls?.stop();
    };
  }, [isSupported]);

  if (cameraError) {
    return <ErrorBanner message={cameraError} />;
  }

  return (
    <div className="relative overflow-hidden rounded-lg bg-black">
      <video ref={videoRef} className="aspect-[4/3] w-full object-cover" muted playsInline />
      {isStarting && (
        <div className="absolute inset-0 flex items-center justify-center text-sm text-white">
          Starting camera...
        </div>
      )}
      {/* Framing guide to help aim at the barcode */}
      <div className="pointer-events-none absolute inset-0 flex items-center justify-center">
        <div className="h-1/3 w-2/3 rounded-lg border-2 border-white/70" />
      </div>
    </div>
  );
}
