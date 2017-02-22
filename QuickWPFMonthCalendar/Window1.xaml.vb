''' <summary>A very quick and dirty WPF Month-view calendar control that supports simple 1-day appointments.
''' This is *NOT* meant to showcase best practices for WPF, or for .Net coding in general.  Please improve it, and post the
''' improvements to CodeProject so that others can benefit, thanks!  Kirk Davis, February 2009, Bangkok, Thailand.
''' </summary>
''' <remarks>
''' ''' This code is for anybody to use for any legal reason.  Given that I wrote this in about four hours, use it at your own risk.
''' If your application crashes, a memory-leak brings down your entire country, or you hate it, you take full responsibility.</remarks>
Class Window1

    Private _myAppointmentsList As New List(Of Appointment.Appointment)


    Private Sub Window1_Loaded(ByVal sender As Object, ByVal e As System.Windows.RoutedEventArgs) Handles Me.Loaded

        Dim rand As New Random(Date.Now.Second)

        For i As Integer = 1 To 50
            Dim apt As New Appointment.Appointment()
            apt.AppointmentID = i
            apt.StartTime = New Date(Date.Now.Year, _
                                     rand.Next(1, 12), _
                                     rand.Next(1, Date.DaysInMonth(Date.Now.Year, Date.Now.Month)))
            apt.EndTime = apt.StartTime
            apt.Subject = "Random apt, blah blah"
            _myAppointmentsList.Add(apt)
        Next

        Call SetAppointments()

    End Sub

    Private Sub DayBoxDoubleClicked_event(ByVal e As NewAppointmentEventArgs) Handles AptCalendar.DayBoxDoubleClicked
        MessageBox.Show("You double-clicked on day " & CDate(e.StartDate).ToShortDateString(), "Calendar Event", MessageBoxButton.OK)
    End Sub

    Private Sub AppointmentDblClicked(ByVal Appointment_Id As Integer) Handles AptCalendar.AppointmentDblClicked
        MessageBox.Show("You double-clicked on appointment with ID = " & Appointment_Id, "Calendar Event", MessageBoxButton.OK)
    End Sub

    Private Sub DisplayMonthChanged(ByVal e As MonthChangedEventArgs) Handles AptCalendar.DisplayMonthChanged
        Call SetAppointments()
    End Sub

    Private Sub SetAppointments()
        '-- Use whatever function you want to load the MonthAppointments list, I happen to have a list filled by linq that has
        '   many (possibly the past several years) of them loaded, so i filter to only pass the ones showing up in the displayed
        '   month.  Note that the "setter" for MonthAppointments also triggers a redraw of the display.
        Me.AptCalendar.MonthAppointments = _myAppointmentsList.FindAll(
                        New System.Predicate(Of Appointment.Appointment)(
                        Function(apt As Appointment.Appointment) _
                            apt.StartTime IsNot Nothing AndAlso
                            CDate(apt.StartTime).Month = Me.AptCalendar.DisplayStartDate.Month AndAlso
                            CDate(apt.StartTime).Year = Me.AptCalendar.DisplayStartDate.Year))
    End Sub

End Class
