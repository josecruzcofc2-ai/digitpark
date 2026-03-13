import axios from 'axios';

type AlertSeverity = 'info' | 'warning' | 'critical';

interface SlackAttachment {
  color: string;
  title: string;
  text: string;
  fields: Array<{ title: string; value: string; short: boolean }>;
}

/**
 * Envia alertas al canal de Slack configurado via webhook.
 * No lanza excepcion si el webhook falla — solo loguea el error.
 */
export async function sendSlackAlert(
  webhookUrl: string,
  message: string,
  severity: AlertSeverity
): Promise<void> {
  if (!webhookUrl) return;

  const severityConfig: Record<AlertSeverity, { emoji: string; color: string }> = {
    critical: { emoji: '[CRITICAL]', color: '#ff0000' },
    warning:  { emoji: '[WARNING]',  color: '#ff9900' },
    info:     { emoji: '[INFO]',     color: '#36a64f' },
  };

  const { emoji, color } = severityConfig[severity];

  const attachment: SlackAttachment = {
    color,
    title: `${emoji} Digit Park Payment Alert`,
    text: message,
    fields: [
      { title: 'Severity', value: severity.toUpperCase(), short: true },
      { title: 'Time',     value: new Date().toISOString(), short: true },
    ],
  };

  try {
    await axios.post(webhookUrl, { attachments: [attachment] }, { timeout: 5000 });
  } catch (err) {
    console.error('[Alert] Error enviando Slack alert:', err);
  }
}
