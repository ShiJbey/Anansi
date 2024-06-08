using System;
using System.Collections.Generic;
using System.Linq;

namespace Anansi
{
	public static class IEnumerableExtensions
	{
		/// <summary>
		/// Perform weighted random selection over items in an IEnumerable
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="sequence"></param>
		/// <param name="weightSelector"></param>
		/// <returns></returns>
		public static T RandomElementByWeight<T>(this IEnumerable<T> sequence, Func<T, float> weightSelector)
		{
			float totalWeight = sequence.Sum( weightSelector );
			// The weight we are after...
			float itemWeightIndex = (float)new System.Random().NextDouble() * totalWeight;
			float currentWeightIndex = 0;

			foreach ( var item in from weightedItem in sequence select new { Value = weightedItem, Weight = weightSelector( weightedItem ) } )
			{
				currentWeightIndex += item.Weight;

				// If we've hit or passed the weight we are after for this item then it's the one we want....
				if ( currentWeightIndex >= itemWeightIndex )
					return item.Value;

			}

			return default;

		}

	}

	public static class ContentSelection
	{
		public static List<T> GetWithTags<T>(
			IEnumerable<(T, HashSet<string>)> items,
			IEnumerable<string> tags
		)
		{
			List<(T, int)> matches = new List<(T, int)>();
			int maxMatchScore = 0;

			HashSet<string> mandatoryTags = new HashSet<string>( tags.Where( t => t[0] != '~' ) );
			HashSet<string> optionalTags = new HashSet<string>( tags.Where( t => t[0] == '~' ) );

			foreach ( var entry in items )
			{
				var unsatisfiedMandatoryTags = mandatoryTags.Except( entry.Item2 );
				var hasAllMandatoryTags = unsatisfiedMandatoryTags.Count() == 0;

				if ( !hasAllMandatoryTags )
				{
					continue;
				}

				var satisfiedOptionalTags = optionalTags.Intersect( entry.Item2 );
				var optionalTagsCount = satisfiedOptionalTags.Count();

				matches.Add( (entry.Item1, optionalTagsCount) );

				maxMatchScore = Math.Max( optionalTagsCount, maxMatchScore );
			}

			if ( matches.Count > 0 )
			{
				matches.Sort( (a, b) =>
				{
					if ( a.Item2 > b.Item2 )
					{
						return 1;
					}
					else if ( a.Item2 == b.Item2 )
					{
						return 0;
					}
					else
					{
						return -1;
					}
				} );

				List<T> bestMatches = matches
					.Where( entry => entry.Item2 == maxMatchScore )
					.Select( entry => entry.Item1 )
					.ToList();

				return bestMatches;
			}

			return new List<T>();
		}
	}
}
