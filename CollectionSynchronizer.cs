﻿using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace JMR.Common
{
    public class CollectionSynchronizer<TSource, TTarget> : IDisposable
    {
        private Func<TTarget, TSource> _sourceCreator;
        private Func<TSource, TTarget> _targetCreator;

        public SynchronizationMode SyncMode { get; private set; }
        public ObservableCollection<TSource> Source { get; private set; }
        public ObservableCollection<TTarget> Target { get; private set; }

        /// <summary>
        /// Used to keep two ObservableCollections in sync.
        /// </summary>
        /// <param name="source">The source collection.</param>
        /// <param name="target">The target collection.</param>
        /// <param name="sourceCreator">Function to create a source type.</param>
        /// <param name="targetCreator">Function to create a target type.</param>
        /// <param name="syncMode">The synchronization mode. (Defaults to TwoWay)</param>
        public CollectionSynchronizer(
            ObservableCollection<TSource> source,
            ObservableCollection<TTarget> target,
            Func<TTarget, TSource> sourceCreator,
            Func<TSource, TTarget> targetCreator,
            SynchronizationMode syncMode = SynchronizationMode.TwoWay)
        {
            ThrowOnNull(source, "source");
            ThrowOnNull(target, "target");
            ThrowOnNull(sourceCreator, "sourceCreator");
            ThrowOnNull(targetCreator, "targetCreator");

            _sourceCreator = sourceCreator;
            _targetCreator = targetCreator;
			
            Source = source;
            Target = target;
            SyncMode = syncMode;

            if (SyncMode == SynchronizationMode.OneWayToTarget ||
                SyncMode == SynchronizationMode.TwoWay)
            {
                Source.CollectionChanged += Source_CollectionChanged;
            }

            if (SyncMode == SynchronizationMode.OneWayToSource ||
                SyncMode == SynchronizationMode.TwoWay)
            {
                Target.CollectionChanged += Target_CollectionChanged;
            }
        }
        private void ThrowOnNull(object arg, string name)
        {
            if (arg == null)
            {
                throw new ArgumentNullException(name);
            }
        }

        public void Dispose()
        {
            Source.CollectionChanged -= Source_CollectionChanged;
            Target.CollectionChanged -= Target_CollectionChanged;
        }

        private void Target_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Source.CollectionChanged -= Source_CollectionChanged;
            HandleChange<TTarget, TSource>(e, Source, Target, _sourceCreator);
            Source.CollectionChanged += Source_CollectionChanged;
        }
        private void Source_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Target.CollectionChanged -= Target_CollectionChanged;
            HandleChange<TSource, TTarget>(e, Target, Source, _targetCreator);
            Target.CollectionChanged += Target_CollectionChanged;
        }

        private void HandleChange<TISource, TITarget>(
            NotifyCollectionChangedEventArgs args,
            ObservableCollection<TITarget> target,
            ObservableCollection<TISource> source,
            Func<TISource, TITarget> creator)
        {
            switch (args.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    target.AddRange(
                        args.NewItems.Cast<TISource>().CreateRange(creator));
                    break;
                case NotifyCollectionChangedAction.Move:
                    target.MoveRange(
                        args.OldStartingIndex, args.OldItems.Count(),
                        args.NewStartingIndex, args.NewItems.Count());
                    break;
                case NotifyCollectionChangedAction.Remove:
                    target.RemoveRange(args.OldStartingIndex, args.OldItems.Count());
                    break;
                case NotifyCollectionChangedAction.Replace:
                    target.RemoveRange(args.OldStartingIndex, args.OldItems.Count());
                    target.InsertRange(args.NewStartingIndex, args.NewItems
						.Cast<TISource>().CreateRange(creator));
                    break;
                case NotifyCollectionChangedAction.Reset:
                    target.Clear();
                    target.AddRange(source.CreateRange(creator));
                    break;
            }
        }
    }
}
