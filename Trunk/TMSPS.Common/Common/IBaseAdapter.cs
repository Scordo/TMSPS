namespace TMSPS.Core.Common
{
    /// <summary>
    /// The inetrface each adapter should implement
    /// </summary>
    public interface IBaseAdapter
    {
        /// <summary>
        /// Starts a transaction.
        /// </summary>
        /// <returns></returns>
        bool BeginTransaction();

        /// <summary>
        /// Commits a running transaction.
        /// </summary>
        void CommitTransaction();

        /// <summary>
        /// Commits a running transaction if the provided parameter is set to false.
        /// </summary>
        /// <param name="transactionExisted">if set to <c>true</c> [transaction existed].</param>
        void CommitTransaction(bool transactionExisted);

        /// <summary>
        /// Rollbacks a running transaction.
        /// </summary>
        void RollbackTransaction();

        /// <summary>
        /// Rollbacks a running transaction if the provided parameter is set to false.
        /// </summary>
        /// <param name="transactionExisted">if set to <c>true</c> [transaction existed].</param>
        void RollbackTransaction(bool transactionExisted);

        /// <summary>
        /// Executes the provided logic in an transaction and conditionally does a rollback and commit
        /// </summary>
        /// <param name="delLogic">The logic to execute in an transaction</param>
        void RunInTransaction(ParameterlessMethodDelegate delLogic);

        /// <summary>
        /// Copies the context from <paramref name="adapter"/> to the current instance. A context is for example a connection for db-adapter or something simmilar.
        /// </summary>
        /// <param name="adapter">The adapter.</param>
        void SetContextFrom(IBaseAdapter adapter);
    }
}