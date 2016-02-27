// DefaultDlg.cpp : 实现文件
//

#include "stdafx.h"
#include "PGCreator.h"
#include "DefaultDlg.h"
#include "afxdialogex.h"


// CDefaultDlg 对话框

IMPLEMENT_DYNAMIC(CDefaultDlg, CDialog)

CDefaultDlg::CDefaultDlg(CWnd* pParent /*=NULL*/)
	: CDialog(CDefaultDlg::IDD, pParent)
{

}

CDefaultDlg::~CDefaultDlg()
{
}

void CDefaultDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	//  DDX_Control(pDX, IDC_RADIO_SMF, m_Rmf);
	//  DDX_Control(pDX, IDC_RADIO_SDCA, m_Rsdc);
}


BEGIN_MESSAGE_MAP(CDefaultDlg, CDialog)
END_MESSAGE_MAP()


// CDefaultDlg 消息处理程序


BOOL CDefaultDlg::PreTranslateMessage(MSG* pMsg)
{
	// TODO: 在此添加专用代码和/或调用基类
	if(pMsg->message==WM_KEYDOWN && pMsg->wParam==VK_ESCAPE ) return TRUE;
	if(pMsg->message==WM_KEYDOWN && pMsg->wParam==VK_RETURN ) return TRUE;
	else return CDialog::PreTranslateMessage(pMsg);
}


BOOL CDefaultDlg::OnInitDialog()
{
	CButton* p_Rmf = (CButton*)GetDlgItem(IDC_RADIO_SMF);
	CButton* p_Rsdc = (CButton*)GetDlgItem(IDC_RADIO_SDCA);
	p_Rmf->SetCheck(TRUE);
	p_Rsdc->SetCheck(TRUE);
	return TRUE;
}

void CDefaultDlg::OutputInfo(FILE* fp)
{
	int mf = GetCheckedRadioButton( IDC_RADIO_SMF, IDC_RADIO_ucMF ) - IDC_RADIO_SMF;
	int sdc = GetCheckedRadioButton( IDC_RADIO_SDCA, IDC_RADIO_SDCF ) - IDC_RADIO_SDCA;
	fprintf_s(fp,"%d\t%d\t\n",mf,sdc);

}