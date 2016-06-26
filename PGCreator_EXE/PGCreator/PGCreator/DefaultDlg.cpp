// DefaultDlg.cpp : ʵ���ļ�
//

#include "stdafx.h"
#include "PGCreator.h"
#include "DefaultDlg.h"
#include "afxdialogex.h"


// CDefaultDlg �Ի���

IMPLEMENT_DYNAMIC(CDefaultDlg, CDialog)

CDefaultDlg::CDefaultDlg(CWnd* pParent /*=NULL*/)
	: CDialog(CDefaultDlg::IDD, pParent)
	, m_EangleTol(0)
	, m_EfloorTol(0)
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
	DDX_Text(pDX, IDC_EDIT_ANGLETOL, m_EangleTol);
	DDX_Text(pDX, IDC_EDIT_FLOORTOL, m_EfloorTol);
}


BEGIN_MESSAGE_MAP(CDefaultDlg, CDialog)
	ON_BN_CLICKED(IDC_BUTTON_NOTEPAD, &CDefaultDlg::OnClickedButtonNotepad)
	ON_EN_KILLFOCUS(IDC_EDIT_ANGLETOL, &CDefaultDlg::OnKillfocusEditAngletol)
	ON_EN_KILLFOCUS(IDC_EDIT_FLOORTOL, &CDefaultDlg::OnKillfocusEditFloortol)
END_MESSAGE_MAP()


// CDefaultDlg ��Ϣ�������


BOOL CDefaultDlg::PreTranslateMessage(MSG* pMsg)
{
	// TODO: �ڴ����ר�ô����/����û���
	if(pMsg->message==WM_KEYDOWN && pMsg->wParam==VK_ESCAPE ) return TRUE;
	if(pMsg->message==WM_KEYDOWN && pMsg->wParam==VK_RETURN ) return TRUE;
	else return CDialog::PreTranslateMessage(pMsg);
}


BOOL CDefaultDlg::OnInitDialog()
{
	CButton* p_Rmf = (CButton*)GetDlgItem(IDC_RADIO_SMF);
	CButton* p_Rsdc = (CButton*)GetDlgItem(IDC_RADIO_SDCA);
	CEdit* p_Eangle = (CEdit*)GetDlgItem(IDC_EDIT_ANGLETOL);
	CEdit* p_Efloor = (CEdit*)GetDlgItem(IDC_EDIT_FLOORTOL);
	p_Rmf->SetCheck(TRUE);
	p_Rsdc->SetCheck(TRUE);
	p_Eangle->SetWindowTextW(L"90.0");
	p_Efloor->SetWindowTextW(L"0.2");
	m_EangleTol = 90.0;
	m_EfloorTol = 0.2;
	m_notepadDlg.ReadExternalSetting();
	return TRUE;
}

void CDefaultDlg::OutputInfo(FILE* fp)
{
	int mf = GetCheckedRadioButton( IDC_RADIO_SMF, IDC_RADIO_ucMF ) - IDC_RADIO_SMF;
	int sdc = GetCheckedRadioButton( IDC_RADIO_SDCA, IDC_RADIO_SDCO ) - IDC_RADIO_SDCA;
	fprintf_s(fp,"%d\t%d\t%f\t%f\t\n",mf,sdc,m_EangleTol,m_EfloorTol);
}

void CDefaultDlg::OnClickedButtonNotepad()
{
	// TODO: �ڴ���ӿؼ�֪ͨ����������
	m_notepadDlg.DoModal();
}

void CDefaultDlg::OutputSetting(FILE* fp)
{
	m_notepadDlg.OutputInfo(fp);
}


void CDefaultDlg::OnKillfocusEditAngletol()
{
	// TODO: �ڴ���ӿؼ�֪ͨ����������
	UpdateData(TRUE);
	if (m_EangleTol < 0 || m_EangleTol > 90)
	{
		CEdit* p_Eangle = (CEdit*)GetDlgItem(IDC_EDIT_ANGLETOL);
		AfxMessageBox(L"�Ƕ����Ӧ��0�㵽90��֮��");
		p_Eangle->SetWindowTextW(L"90.0");
	}
}


void CDefaultDlg::OnKillfocusEditFloortol()
{
	// TODO: �ڴ���ӿؼ�֪ͨ����������
	UpdateData(TRUE);
	if (m_EfloorTol < 0 || m_EfloorTol >= 0.5)
	{
		CEdit* p_Efloor = (CEdit*)GetDlgItem(IDC_EDIT_FLOORTOL);
		AfxMessageBox(L"������Ӧ��0��0.5֮��");
		p_Efloor->SetWindowTextW(L"0.2");
	}
}
