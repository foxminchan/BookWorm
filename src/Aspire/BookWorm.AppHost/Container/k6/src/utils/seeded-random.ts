/**
 * Seeded random number generator for reproducible tests
 * This is safe for performance testing as it's not used for security purposes
 */
export class SeededRandom {
  private seed: number;

  constructor(seed: number = 12345) {
    this.seed = seed;
  }

  /**
   * Linear congruently generator - simple and sufficient for test data generation
   */
  next(): number {
    this.seed = (this.seed * 1664525 + 1013904223) % 4294967296;
    return this.seed / 4294967296;
  }

  /**
   * Generate random integer between min and max (inclusive)
   */
  int(min: number, max: number): number {
    return Math.floor(this.next() * (max - min + 1)) + min;
  }

  /**
   * Generate random boolean with given probability
   */
  bool(probability: number = 0.5): boolean {
    return this.next() < probability;
  }

  /**
   * Pick random element from array
   */
  pick<T>(array: T[]): T {
    return array[Math.floor(this.next() * array.length)];
  }
}
