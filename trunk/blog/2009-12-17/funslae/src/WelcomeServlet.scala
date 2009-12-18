package funslae
  
import javax.servlet.http._

class WelcomeServlet extends HttpServlet  {

  override def doGet(request:HttpServletRequest, response:HttpServletResponse) = {
    response.setContentType("text/plain")
    response.getWriter.print("Welcome to my world!")
  }
  
}
