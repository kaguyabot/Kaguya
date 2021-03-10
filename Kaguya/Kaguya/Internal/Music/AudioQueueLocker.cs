namespace Kaguya.Internal.Music
{
	public class AudioQueueLocker
	{
		public object Locker = new();
	}
}