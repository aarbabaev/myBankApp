import { Component, signal, computed, effect, OnDestroy } from '@angular/core';
import { DecimalPipe } from '@angular/common';

export type Mode = 'pomodoro' | 'short' | 'long';

const DURATIONS: Record<Mode, number> = {
  pomodoro: 25 * 60,
  short: 5 * 60,
  long: 15 * 60,
};

const CIRCUMFERENCE = 2 * Math.PI * 88;

@Component({
  selector: 'app-root',
  imports: [DecimalPipe],
  templateUrl: './app.html',
  styleUrl: './app.scss',
})
export class App implements OnDestroy {
  readonly modes: Mode[] = ['pomodoro', 'short', 'long'];
  readonly modeLabels: Record<Mode, string> = {
    pomodoro: 'Focus',
    short: 'Short Break',
    long: 'Long Break',
  };

  mode = signal<Mode>('pomodoro');
  remaining = signal<number>(DURATIONS['pomodoro']);
  running = signal(false);
  sessions = signal(0);

  private intervalId: ReturnType<typeof setInterval> | null = null;

  total = computed(() => DURATIONS[this.mode() as Mode]);

  minutes = computed(() => Math.floor(this.remaining() / 60));
  seconds = computed(() => this.remaining() % 60);

  ringOffset = computed(() => {
    const progress = 1 - this.remaining() / this.total();
    return CIRCUMFERENCE * progress;
  });

  isBreak = computed(() => this.mode() !== 'pomodoro');

  constructor() {
    effect(() => {
      const min = this.minutes().toString().padStart(2, '0');
      const sec = this.seconds().toString().padStart(2, '0');
      document.title = `${min}:${sec} — Pomodoro`;
    });
  }

  setMode(m: Mode): void {
    this.stopTimer();
    this.mode.set(m);
    this.remaining.set(DURATIONS[m]);
    this.running.set(false);
  }

  toggleTimer(): void {
    if (this.running()) {
      this.stopTimer();
      this.running.set(false);
    } else {
      if (Notification.permission === 'default') {
        Notification.requestPermission();
      }
      this.intervalId = setInterval(() => this.tick(), 1000);
      this.running.set(true);
    }
  }

  reset(): void {
    this.setMode(this.mode());
  }

  private tick(): void {
    const current = this.remaining();
    if (current <= 0) {
      this.stopTimer();
      this.running.set(false);
      if (this.mode() === 'pomodoro') {
        this.sessions.update((s: number) => s + 1);
        this.notify('Focus session done! Time for a break.');
      } else {
        this.notify('Break over! Ready to focus?');
      }
      return;
    }
    this.remaining.set(current - 1);
  }

  private stopTimer(): void {
    if (this.intervalId !== null) {
      clearInterval(this.intervalId);
      this.intervalId = null;
    }
  }

  private notify(msg: string): void {
    if (Notification.permission === 'granted') {
      new Notification('Pomodoro', { body: msg });
    }
  }

  ngOnDestroy(): void {
    this.stopTimer();
  }
}
