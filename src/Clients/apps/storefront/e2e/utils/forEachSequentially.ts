export function forEachSequentially<T>(
  items: readonly T[],
  action: (item: T, index: number) => Promise<void>,
): Promise<void> {
  return items.reduce<Promise<void>>(
    (previous, item, index) => previous.then(() => action(item, index)),
    Promise.resolve(),
  );
}
