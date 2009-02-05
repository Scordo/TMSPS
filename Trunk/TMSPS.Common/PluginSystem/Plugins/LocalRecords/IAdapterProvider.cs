using TMSPS.Core.Common;

namespace TMSPS.Core.PluginSystem.Plugins.LocalRecords
{
    /// <summary>
    /// This interface must be implemented by adapter providers
    /// </summary>
    public interface IAdapterProvider
    {
        /// <summary>
        /// Inits adapter with the specified parameter
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        void Init(string parameter);

        /// <summary>
        /// Gets the challenge adapter.
        /// </summary>
        /// <returns></returns>
        IChallengeAdapter GetChallengeAdapter();

        /// <summary>
        /// Gets the challenge adapter.
        /// </summary>
        /// <param name="adapterToCopyContextFrom">The adapter to copy context from.</param>
        /// <returns></returns>
        IChallengeAdapter GetChallengeAdapter(IBaseAdapter adapterToCopyContextFrom);


        /// <summary>
        /// Gets the player adapter.
        /// </summary>
        /// <returns></returns>
        IPlayerAdapter GetPlayerAdapter();

        /// <summary>
        ///  Gets the player adapter.
        /// </summary>
        /// <param name="adapterToCopyContextFrom">The adapter to copy context from.</param>
        /// <returns></returns>
        IPlayerAdapter GetPlayerAdapter(IBaseAdapter adapterToCopyContextFrom);

        /// <summary>
        /// Gets the position adapter.
        /// </summary>
        /// <returns></returns>
        IPositionAdapter GetPositionAdapter();

        /// <summary>
        /// Gets the position adapter.
        /// </summary>
        /// <param name="adapterToCopyContextFrom">The adapter to copy context from.</param>
        /// <returns></returns>
        IPositionAdapter GetPositionAdapter(IBaseAdapter adapterToCopyContextFrom);

        /// <summary>
        /// Gets the rank adapter.
        /// </summary>
        /// <returns></returns>
        IRankAdapter GetRankAdapter();

        /// <summary>
        /// Gets the rank adapter.
        /// </summary>
        /// <param name="adapterToCopyContextFrom">The adapter to copy context from.</param>
        /// <returns></GetRankAdapter>
        IRankAdapter GetRankAdapter(IBaseAdapter adapterToCopyContextFrom);

        /// <summary>
        /// Gets the rating adapter.
        /// </summary>
        /// <returns></returns>
        IRatingAdapter GetRatingAdapter();

        /// <summary>
        /// Gets the rating adapter.
        /// </summary>
        /// <param name="adapterToCopyContextFrom">The adapter to copy context from.</param>
        /// <returns></GetRankAdapter>
        IRatingAdapter GetRatingAdapter(IBaseAdapter adapterToCopyContextFrom);

        /// <summary>
        /// Gets the record adapter.
        /// </summary>
        /// <returns></returns>
        IRecordAdapter GetRecordAdapter();

        /// <summary>
        /// Gets the record adapter.
        /// </summary>
        /// <param name="adapterToCopyContextFrom">The adapter to copy context from.</param>
        /// <returns></GetRankAdapter>
        IRecordAdapter GetRecordAdapter(IBaseAdapter adapterToCopyContextFrom);

        /// <summary>
        /// Gets the session adapter.
        /// </summary>
        /// <returns></returns>
        ISessionAdapter GetSessionAdapter();

        /// <summary>
        /// Gets the session adapter.
        /// </summary>
        /// <param name="adapterToCopyContextFrom">The adapter to copy context from.</param>
        /// <returns></GetRankAdapter>
        ISessionAdapter GetSessionAdapter(IBaseAdapter adapterToCopyContextFrom);
    }
}
