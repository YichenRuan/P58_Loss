
// PGCreator.h : PROJECT_NAME Ӧ�ó������ͷ�ļ�
//

#pragma once

#ifndef __AFXWIN_H__
	#error "�ڰ������ļ�֮ǰ������stdafx.h�������� PCH �ļ�"
#endif

#include "resource.h"		// ������


// CPGCreatorApp:
// �йش����ʵ�֣������ PGCreator.cpp
//

class CPGCreatorApp : public CWinApp
{
public:
	CPGCreatorApp();

// ��д
public:
	virtual BOOL InitInstance();

// ʵ��

	DECLARE_MESSAGE_MAP()
};

extern CPGCreatorApp theApp;