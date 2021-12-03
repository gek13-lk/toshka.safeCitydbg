namespace Toshka.dbgSave.NeuralNetwork.Events
{
    public class DomainObject
    {
        public virtual long Id { get; set; }

        public bool IsNew => Id <= 0;
    }
}