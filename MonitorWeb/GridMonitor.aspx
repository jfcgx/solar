<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="GridMonitor.aspx.cs" Inherits="MonitorWeb.GridMonitor" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    </br>
    <asp:Timer runat="server" ID="UP_Timer" Interval="1000" />
        <asp:UpdatePanel runat="server" ID="Game_UpdatePanel">
            <Triggers>
                <asp:AsyncPostBackTrigger ControlID="UP_Timer" EventName="Tick" />
            </Triggers>
            <ContentTemplate>
                <asp:Repeater runat="server" ID="BoardRepeater" ItemType="Field.TextFields">
                    <HeaderTemplate>
                        <table border="1" style="font-family: Arial; font-size: x-small; background-color: #CCCCFF; ">
                            <tr>
                                <th>Lectura</th>
                            </tr>
                    </HeaderTemplate>
                    <ItemTemplate>
                        <tr>
                            <td> <asp:TextBox ID="TextBox1" runat="server" Font-Names="Consolas" Height="400px" TextMode="MultiLine" Width="300px" Text="<%#:  Item.Contenido %>"></asp:TextBox></td>
                        </tr>
                    </ItemTemplate>
                    <FooterTemplate></table></FooterTemplate>
                </asp:Repeater>
            </ContentTemplate>
        </asp:UpdatePanel>
     <p>
         </p>
     <table >
        <tr>
            <td colspan="2" style="height: 20px">BOMBAS</td>
        </tr>
        <tr>
            <td style="width: 89px">Led Rio</td>
            <td style="width: 312px">
                <asp:Button ID="Button1" runat="server" OnClick="Button1_Click" Text="Toggle" />
            </td>
        </tr>
        <tr>
            <td style="width: 89px">Riego</td>
            <td style="width: 312px">
                <asp:Button ID="Button2" runat="server" OnClick="Button2_Click" Text="Toggle" />
            </td>
        </tr>
        <tr>
            <td style="width: 89px">Filtro Piscina</td>
            <td style="width: 312px">
                <asp:Button ID="Button3" runat="server" OnClick="Button3_Click" Text="Toggle" />
            </td>
        </tr>
          <tr>
            <td style="width: 89px">Manual</td>
            <td style="width: 312px">
                <asp:Button ID="Button4" runat="server" OnClick="Button4_Click" Text="Toggle" />
            </td>
        </tr>
    </table> 
    </br>
  
    <table border="1" style="font-family: Consolas; font-size: x-small" >
        <tr dir="auto">
            <td class="modal-sm" style="width: 115px; height: 15px;"></td>
            <td style="width: 155px; height: 15px;" dir="auto">Mes Anterior</td>
            <td style="width: 155px; height: 15px;" dir="auto">Mes Actual</td>
        </tr>
        <tr>
            <td class="modal-sm" style="width: 115px; height: 15px;">Promedio</td>
            <td style="width: 155px; height: 15px;" dir="rtl">
                <asp:Label ID="lblPromedio" runat="server" Text="Label"></asp:Label>
            </td>
            <td style="width: 155px; height: 15px" dir="rtl">
                <asp:Label ID="lblPromedioActual" runat="server" Text="Label"></asp:Label>
            </td>
        </tr>
        <tr>
            <td class="modal-sm" style="width: 115px">Max</td>
            <td style="width: 155px" dir="rtl">
                <asp:Label ID="max" runat="server" Text="Label"></asp:Label>
            </td>
            <td style="width: 155px" dir="rtl">
                <asp:Label ID="maxActual" runat="server" Text="Label"></asp:Label>
            </td>
        </tr>
        <tr>
            <td class="modal-sm" style="width: 115px">Producción FV</td>
            <td style="width: 155px" dir="rtl">
                <asp:Label ID="produccion" runat="server" Text="Label"></asp:Label>
            </td>
            <td style="width: 155px" dir="rtl">
                <asp:Label ID="produccionActual" runat="server" Text="Label"></asp:Label>
            </td>
        </tr>
        <tr>
            <td class="modal-sm" style="width: 115px">Exportado</td>
            <td style="width: 155px" dir="rtl">
                <asp:Label ID="lblExportado" runat="server" Text="Label"></asp:Label>
            </td>
            <td style="width: 155px" dir="rtl">
                <asp:Label ID="lblExportadoActual" runat="server" Text="Label"></asp:Label>
            </td>
        </tr>
        <tr>
            <td class="modal-sm" style="width: 115px">Importado</td>
            <td style="width: 155px" dir="rtl">
                <asp:Label ID="lblImportado" runat="server" Text="Label"></asp:Label>
            </td>
            <td style="width: 155px" dir="rtl">
                <asp:Label ID="lblImportadoActual" runat="server" Text="Label"></asp:Label>
            </td>
        </tr>
        <tr>
            <td class="modal-sm" style="width: 115px">Facturado</td>
            <td style="width: 155px" dir="rtl">
                <asp:Label ID="Facturado" runat="server" Text="Label"></asp:Label>
            </td>
            <td style="width: 155px" dir="rtl">
                <asp:Label ID="FacturadoActual" runat="server" Text="Label"></asp:Label>
            </td>
        </tr>
        <tr>
            <td class="modal-sm" style="width: 115px">Total</td>
            <td style="width: 155px" dir="rtl">
                <asp:Label ID="Total" runat="server" Text="Label"></asp:Label>
            </td>
            <td style="width: 155px" dir="rtl">
                <asp:Label ID="TotalActual" runat="server" Text="Label"></asp:Label>
            </td>
        </tr>
    </table>

</asp:Content>
