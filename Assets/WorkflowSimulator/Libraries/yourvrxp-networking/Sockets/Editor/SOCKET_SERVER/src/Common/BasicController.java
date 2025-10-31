package Common;

import java.lang.reflect.InvocationTargetException;
import java.lang.reflect.Method;
import java.util.Vector;
import java.util.function.Function;

// ----------------------------------------------
/*
 * Singleton that will dispatch events to the registered
 * callback functions
 */	
public class BasicController extends Thread
{
   private boolean m_keepRunning = true;
   private Vector<BasicRegister> m_listenerArray = new Vector<BasicRegister>();	// Registered listeners
   private Vector<BasicEvent> m_eventsDelayed = new Vector<BasicEvent>();		// Events delayed	

   // ----------------------------------------------
   /*
    * Constructor
    */	
   private static BasicController instance = null;
   protected BasicController() {
      // Exists only to defeat instantiation.
   }
	
   // ----------------------------------------------
   /*
    * Reference to get the singleton
    */	
   public static BasicController getInstance() {
      if(instance == null) {
         instance = new BasicController();
         instance.start();
      }
      return instance;
   }
   	
   // ----------------------------------------------
   /*
    * Add a new callback function to listen the events
    */	
	public void AddMyEventListener( Object _object, String _name )
	{
		try {
			Method OnBasicEventMethod = _object.getClass().getDeclaredMethod(_name, BasicEvent.class);
			m_listenerArray.addElement( new BasicRegister( _object, OnBasicEventMethod, _name) );
		} catch (Exception e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		}	
	}

   // ----------------------------------------------
   /*
    * Add a new callback function to listen the events
    */	
	public boolean RemoveMyEventListener( Object _object, String _name )
	{
		for (int i = 0; i < m_listenerArray.size(); i++)
		{
			BasicRegister item = m_listenerArray.elementAt(i);
			if (item.IsEqual(_object, _name))
			{
				m_listenerArray.removeElementAt(i);
				return true;
			}
		}
		return false;
	}

   // ----------------------------------------------
   /*
    * Dispatch an event to all the listeners registered
    */	
	public void DispatchMyEvent( String _event, Object... _parameters)
	{
		BasicEvent newEvent = new BasicEvent(_event, -1, _parameters);
		for (int i = 0; i < m_listenerArray.size(); i++)
		{
			BasicRegister item = m_listenerArray.elementAt(i);
			try {
				item.GetMethod().invoke(item.GetObject(), newEvent);
			} catch (Exception e) {
				// TODO Auto-generated catch block
				e.printStackTrace();
			}
		}		
	}

   // ----------------------------------------------
   /*
    * Dispatch an event to all the listeners registered
    */	
	public void DispatchMyEvent( BasicEvent _event )
	{ 
		for (int i = 0; i < m_listenerArray.size(); i++)
		{
			BasicRegister item = m_listenerArray.elementAt(i);
			try {
				item.GetMethod().invoke(item.GetObject(), _event);
			} catch (Exception e) {
				// TODO Auto-generated catch block
				e.printStackTrace();
			}
		}		
	}
	
   // ----------------------------------------------
   /*
    * Push to the queue an event to be dispatched in the future
    */	
	public void DelayMyEvent( String _event, int _delay, Object... _parameters)
	{
		BasicEvent newEvent = new BasicEvent(_event, _delay, _parameters);
		m_eventsDelayed.add(newEvent);
	}
	
    // ----------------------------------------------
    /*
     * Will keep updating the delayed event and dispatching them
     */
	@Override
	public void run() 
	{
		String data = null;
		while (m_keepRunning)
		{
			try 
        	{
				wait(10);
        	} catch (Exception err) {};
        	
        	// Update delayed events
        	for (int i = 0; i < m_eventsDelayed.size(); i++)
        	{
        		BasicEvent sEvent = m_eventsDelayed.elementAt(i);
        		if (sEvent.UpdateDelay())
        		{
        			m_eventsDelayed.removeElementAt(i);
        			DispatchMyEvent(sEvent);
        			i--;
        		}
        	}
		}
	}
}
