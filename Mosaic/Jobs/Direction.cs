using System;
using System.Collections.Generic;

namespace Mosaic.Jobs {
    internal enum Direction {
        Left,
        Right,
        Top,
        Bottom
    }

    internal interface IDirection<out T> {
        T Left { get; }
        T Right { get; }
        T Top { get; }
        T Bottom { get; }
    }

    internal static class DirectionsExtensions {
        public static Direction GetOppositeDirection(this Direction self) {
            switch (self) {
                case Direction.Left:
                    return Direction.Right;
                case Direction.Right:
                    return Direction.Left;
                case Direction.Top:
                    return Direction.Bottom;
                case Direction.Bottom:
                    return Direction.Top;
                default:
                    throw new ArgumentOutOfRangeException(nameof(self), self, null);
            }
        }

        public static T GetByDirection<T>(this IDirection<T> self, Direction direction) {
            switch (direction) {
                case Direction.Left:
                    return self.Left;
                case Direction.Right:
                    return self.Right;
                case Direction.Top:
                    return self.Top;
                case Direction.Bottom:
                    return self.Bottom;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }
        }

        public static T GetByOppositeDirection<T>(this IDirection<T> self, Direction direction) {
            switch (direction) {
                case Direction.Left:
                    return self.Right;
                case Direction.Right:
                    return self.Left;
                case Direction.Top:
                    return self.Bottom;
                case Direction.Bottom:
                    return self.Top;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }
        }

        public static IEnumerable<T> GetAllDirections<T>(this IDirection<T> self) {
            yield return self.Left;
            yield return self.Top;
            yield return self.Right;
            yield return self.Bottom;
        }
    }
}