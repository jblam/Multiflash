using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace JBlam.Multiflash.Helpers
{
    public class FairPanel : Panel
    {
        public Orientation Orientation { get; set; }
        protected override bool HasLogicalOrientation => true;
        protected override Orientation LogicalOrientation => Orientation;

        protected IEnumerable<UIElement> ChildElements => Children.Cast<UIElement>();

        protected override Size MeasureOverride(Size availableSize)
        {
            var naiveSize = ChildElements.Aggregate(new Size(), (prev, cur) =>
            {
                cur.Measure(availableSize);
                return SizeFromDimensions(
                    PrimaryDimension(prev) + PrimaryDimension(cur.DesiredSize),
                    Math.Max(SecondaryDimension(prev), SecondaryDimension(cur.DesiredSize)));
            });
            return new Size(Math.Min(availableSize.Width, naiveSize.Width), Math.Min(availableSize.Height, naiveSize.Height));
        }

        (double primary, double secondary) GetDimensions(Size size) => Orientation == Orientation.Horizontal
            ? (size.Width, size.Height)
            : (size.Height, size.Width);
        double PrimaryDimension(Size size) => GetDimensions(size).primary;
        double SecondaryDimension(Size size) => GetDimensions(size).secondary;
        Size SizeFromDimensions(double primary, double secondary) => Orientation == Orientation.Horizontal
            ? new Size(primary, secondary)
            : new Size(secondary, primary);
        Rect RectFromDimensions(double primaryStart, double primaryLength, double secondaryLength)
        {
            var start = Orientation == Orientation.Horizontal
                ? new Point(primaryStart, 0)
                : new Point(0, primaryStart);
            var size = SizeFromDimensions(primaryLength, secondaryLength);
            return new Rect(start, size);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            int remainingElements = Children.Count;
            var (totalPrimarySpace, totalSecondarySpace) = GetDimensions(finalSize);

            // algo:
            // an element may fit in its entirety if every smaller element also fits.
            // the remaining "large" elements share space proportionally.

            var allocated = new double[Children.Count];

            var elements = ChildElements.Select((element, index) => (index, desired: GetDimensions(element.DesiredSize))).ToList();
            elements.Sort((u, v) => u.desired.primary.CompareTo(v.desired.primary));
            var totalDesiredSize = elements.Sum(t => t.desired.primary);
            {
                var accumulatedSize = 0.0;
                foreach (var (index, desired, remainingCount) in elements.Select((el, orderedIndex) => (el.index, el.desired.primary, elements.Count - orderedIndex)))
                {
                    var remainingAvailableSpace = totalPrimarySpace - accumulatedSize;
                    var remainingProportionalSpace = (totalPrimarySpace - accumulatedSize) / remainingCount;
                    if (desired < remainingProportionalSpace)
                    {
                        allocated[index] = desired;
                        accumulatedSize += desired;
                    }
                    else
                    {
                        var remainingDesiredSpace = totalDesiredSize - accumulatedSize;
                        var proportionalFraction = desired / remainingDesiredSpace;
                        allocated[index] = proportionalFraction * remainingAvailableSpace;
                    }
                }
            }
            {
                var accumulatedSize = 0.0;
                for (int i = 0; i < Children.Count; i++)
                {
                    Children[i].Arrange(RectFromDimensions(accumulatedSize, allocated[i], totalSecondarySpace));
                    accumulatedSize += allocated[i];
                }
                return SizeFromDimensions(accumulatedSize, totalSecondarySpace);
            }
        }
    }
}
