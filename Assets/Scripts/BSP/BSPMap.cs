﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class BSPMap
{
    private BinaryReader bspFile;

    public BSPHeader header;
    public List<BSPEntity> entities;
    public List<BSPPlane> planes;
    public List<Vector3> vertices;
    public List<BSPNode> nodes;
    public List<BSPTexture> textures;
    public List<BSPTextureSurface> textureSurfaces;
    public List<BSPFace> faces;
    public List<int> faceList;
    public List<BSPLeaf> leaves;
    public List<BSPEdge> edges;
    public List<int> edgeList;
    public List<BSPModel> models;
    private BSPPalette palette;

    public BSPMap( string mapFileName )
    {
        palette = new BSPPalette("palette.lmp");
        bspFile = new BinaryReader( File.Open("Assets/Resources/Maps/" + mapFileName, FileMode.Open) );
        header = new BSPHeader( bspFile );

        LoadEntities( bspFile );
        LoadPlanes( bspFile );
        LoadVertices( bspFile );
        LoadNodes( bspFile );
        LoadTextures( bspFile );
        LoadTextureInfo( bspFile );
        LoadFaces( bspFile );
        LoadLeaves( bspFile );
        LoadEdges( bspFile );
        LoadModels( bspFile );
      
        bspFile.Close();
    }
	
    private void LoadEntities( BinaryReader bspFile )
    {
        entities = new List<BSPEntity>();

        BSPDirectoryEntry entitiesEntry = header.GetDirectoryEntry( DIRECTORY_ENTRY.ENTITIES );        
        bspFile.BaseStream.Seek( entitiesEntry.fileOffset, SeekOrigin.Begin );

        string entityText = new string(  bspFile.ReadChars( entitiesEntry.size ) );        

        StringReader reader = new StringReader(entityText);
        {
            string line = string.Empty;
            do
            {                
                line = reader.ReadLine();
                if ( line != null )
                {
                    if ( line == "{" )                    
                        entities.Add( new BSPEntity( reader ) );                    
                }
            } while ( line != null );
        }        
    }

    private void LoadPlanes(BinaryReader bspFile )
    {
        planes = new List<BSPPlane>();

        BSPDirectoryEntry planesEntry = header.GetDirectoryEntry( DIRECTORY_ENTRY.PLANES );
        long planeCount = planesEntry.size / 20;

        bspFile.BaseStream.Seek(planesEntry.fileOffset, SeekOrigin.Begin);

        for (int i = 0; i < planeCount; i++)        
            planes.Add( new BSPPlane( bspFile ) );        
    }

    private void LoadVertices( BinaryReader bspFile )
    {        
    	vertices = new List<Vector3>();

		BSPDirectoryEntry verticesEntry = header.GetDirectoryEntry( DIRECTORY_ENTRY.MAP_VERTICES );
		int vertCount = verticesEntry.size / 12;

		bspFile.BaseStream.Seek( verticesEntry.fileOffset , SeekOrigin.Begin );

		for ( int i = 0; i < vertCount; i++ )
		{
            // Read vertex and flip Y/Z to match Quake 1
            float x = bspFile.ReadSingle();
            float y = bspFile.ReadSingle();
            float z = bspFile.ReadSingle();

            vertices.Add( new Vector3( x, z, y ) );
		}
    }

    private void LoadNodes( BinaryReader bspFile )
    {
        nodes = new List<BSPNode>();

        BSPDirectoryEntry nodesEntry = header.GetDirectoryEntry( DIRECTORY_ENTRY.NODES );
        int nodeCount = nodesEntry.size / 36;

        bspFile.BaseStream.Seek( nodesEntry.fileOffset, SeekOrigin.Begin );

        for ( int i = 0; i < nodeCount; i++ )
        {
            nodes.Add (new BSPNode( bspFile ) );
        }

    }

    private void LoadTextures( BinaryReader bspFile )
    {
        textures = new List<BSPTexture>();

        BSPDirectoryEntry texturesEntry = header.GetDirectoryEntry( DIRECTORY_ENTRY.WALL_TEXTURES );
        bspFile.BaseStream.Seek( texturesEntry.fileOffset, SeekOrigin.Begin );

        int textureCount = bspFile.ReadInt32();

        int[] offsets = new int[textureCount];

        for ( int i = 0; i < textureCount; i++ )        
            offsets[ i ] = bspFile.ReadInt32();    
        
        for ( int i = 0; i < textureCount; i++ )
        {
            bspFile.BaseStream.Seek( texturesEntry.fileOffset + offsets[ i ], SeekOrigin.Begin );
            textures.Add( new BSPTexture( bspFile, palette ) );
        }
    }

    private void LoadTextureInfo( BinaryReader bspFile )
    {
        textureSurfaces = new List<BSPTextureSurface>();

        BSPDirectoryEntry texInfoEntry = header.GetDirectoryEntry( DIRECTORY_ENTRY.FACE_TEXTURE_INFO );
        bspFile.BaseStream.Seek(texInfoEntry.fileOffset, SeekOrigin.Begin);

        int texInfoCount = texInfoEntry.size / 40;

        for ( int i = 0; i < texInfoCount; i++ )
        {
            textureSurfaces.Add( new BSPTextureSurface( bspFile ) );
        }
    }

    private void LoadFaces( BinaryReader bspFile )
    {
        // Faces
        faces = new List<BSPFace>();

        BSPDirectoryEntry facesEntry = header.GetDirectoryEntry( DIRECTORY_ENTRY.FACES );
        int faceCount = facesEntry.size / 20;

        bspFile.BaseStream.Seek( facesEntry.fileOffset, SeekOrigin.Begin );

        for (int i = 0; i < faceCount; i++)
        {
            faces.Add( new BSPFace( bspFile ) );
        }

        // Face list
        faceList = new List<int>();

        BSPDirectoryEntry faceListEntry = header.GetDirectoryEntry( DIRECTORY_ENTRY.FACE_LIST );
        int faceListCount = faceListEntry.size / 4;

        bspFile.BaseStream.Seek(faceListEntry.fileOffset, SeekOrigin.Begin);

        for (int i = 0; i < faceListCount; i++)
        {
            faceList.Add( bspFile.ReadInt32() );
        }
    }

    private void LoadLeaves( BinaryReader bspFile )
    {
        leaves = new List<BSPLeaf>();

        BSPDirectoryEntry leafEntry = header.GetDirectoryEntry( DIRECTORY_ENTRY.LEAVES );
        long leafCount = leafEntry.size / 28;

        bspFile.BaseStream.Seek( leafEntry.fileOffset, SeekOrigin.Begin );

        for ( int i = 0; i < leafCount; i++ )
            leaves.Add( new BSPLeaf( bspFile ) );
    }
	
    private void LoadEdges( BinaryReader bspFile )
    {
        // Edges
    	edges = new List<BSPEdge>();

		BSPDirectoryEntry edgesEntry = header.GetDirectoryEntry( DIRECTORY_ENTRY.EDGES );
		int edgeCount = edgesEntry.size / 4;

		bspFile.BaseStream.Seek( edgesEntry.fileOffset , SeekOrigin.Begin );

		for ( int i = 0; i < edgeCount; i++ )
		{
			edges.Add( new BSPEdge( bspFile.ReadUInt16(), bspFile.ReadUInt16() ) );
		}

        // Edge list
        edgeList = new List<int>();

        BSPDirectoryEntry edgeListEntry = header.GetDirectoryEntry( DIRECTORY_ENTRY.EDGE_LIST );
        long edgeListCount = edgeListEntry.size / 4;

        bspFile.BaseStream.Seek(edgeListEntry.fileOffset, SeekOrigin.Begin);

        for (int i = 0; i < edgeListCount; i++)
        {
            edgeList.Add(bspFile.ReadInt32());
        }
    }

    private void LoadModels( BinaryReader bspFile )
    {
    	models = new List<BSPModel>();

		BSPDirectoryEntry modelEntry = header.GetDirectoryEntry( DIRECTORY_ENTRY.MODELS );
		long modelCount = modelEntry.size / 64;

    	bspFile.BaseStream.Seek( modelEntry.fileOffset , SeekOrigin.Begin );

    	for ( int i = 0; i < modelCount; i++ )
    	{
    		BSPModel model = new BSPModel( bspFile );	
    		models.Add( model );
    	}
    }
}
