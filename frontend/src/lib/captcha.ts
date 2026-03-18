import { sha256 } from 'js-sha256';

// ── Tipos ─────────────────────────────────────────────────

export interface CaptchaChallenge {
  question:               string;
  recommended_difficulty: number;
}

export interface VerifyArray {
  question: string;
  time:     number;
  nonce:    number;
}

// ── Utilidades ────────────────────────────────────────────

/**
 * Verifica si un número es primo.
 * Usa sqrt para eficiencia — O(sqrt(n)).
 */
export function isPrime(n: number): boolean {
  if (n < 2)  return false;
  if (n === 2) return true;
  if (n % 2 === 0) return false;
  const sqrt = Math.sqrt(n);
  for (let i = 3; i <= sqrt; i += 2) {
    if (n % i === 0) return false;
  }
  return true;
}

/**
 * Resuelve una iteración del PoW del SIMIT.
 * Busca el siguiente nonce primo tal que
 * SHA256(JSON({question, time, nonce})) empiece con "0000".
 * Orden de propiedades crítico: question, time, nonce.
 */
export function solvePoWSingle(
  question: string,
  time:     number,
  startNonce: number = 1
): number {
  let nonce = startNonce + 1;

  while (nonce < 10_000_000) {
    if (isPrime(nonce)) {
      // Orden exacto igual que el captcha-worker.js
      const verifyJson = JSON.stringify({ question, time, nonce });
      const hash       = sha256(verifyJson);

      if (hash.startsWith('0000')) {
        return nonce;
      }
    }
    nonce++;
  }

  throw new Error(`Could not solve PoW in 10M iterations`);
}

/**
 * Obtiene el challenge del servidor del captcha.
 * Usa fetch directamente — el navegador tiene TLS real.
 */
export async function getCaptchaChallenge(): Promise<CaptchaChallenge> {
  const formData = new FormData();
  formData.append('endpoint', 'question');

  const response = await fetch('https://qxcaptcha.fcm.org.co/api.php', {
    method:  'POST',
    headers: {
      'Origin':  'https://www.fcm.org.co',
      'Referer': 'https://www.fcm.org.co/',
    },
    body: formData,
  });

  if (!response.ok) {
    throw new Error(`Captcha server error: ${response.status}`);
  }

  const data = await response.json();

  if (data.error) {
    throw new Error(`Captcha error: ${JSON.stringify(data)}`);
  }

  return data.data as CaptchaChallenge;
}

/**
 * Resuelve el captcha completo y retorna el token.
 * 1. Obtiene el challenge del servidor.
 * 2. Resuelve difficulty veces con SHA256 + primos.
 * 3. Retorna el token JSON serializado como string.
 */
export async function solveCaptcha(): Promise<string> {
  const challenge  = await getCaptchaChallenge();
  const difficulty = challenge.recommended_difficulty || 2;
  const time       = Math.floor(Date.now() / 1000);

  console.log('Challenge:', challenge.question, 'Difficulty:', difficulty);
  const start = Date.now();

  const verification: VerifyArray[] = [];
  let lastNonce = 1;

  for (let i = 0; i < difficulty; i++) {
    lastNonce = solvePoWSingle(challenge.question, time, lastNonce);
    console.log(`Iteration ${i + 1}: nonce=${lastNonce}, time=${Date.now() - start}ms`);
    verification.push({ question: challenge.question, time, nonce: lastNonce });
  }

  console.log(`Total captcha time: ${Date.now() - start}ms`);
  return JSON.stringify(verification);
}